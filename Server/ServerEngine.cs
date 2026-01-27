using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json;
using System.Text;
using System.Globalization;
using AgarGame.Shared;

namespace AgarGame.Server;

/// <summary>
/// manages TCP connections, broadcasts game state, and coordinates threads
/// </summary>
public class ServerEngine
{
    private readonly TcpListener _listener;
    private readonly GameLogic _gameLogic;
    private readonly List<TcpClient> _clients = [];
    private readonly JsonSerializerOptions _jsonOptions = new() { IncludeFields = true };
    private readonly ConcurrentDictionary<string, Vector2> _playerTargets = new();

    public ServerEngine()
    {
        _listener = new TcpListener(IPAddress.Any, GameConstants.Port);
        _gameLogic = new GameLogic(_playerTargets);
    }

    /// <summary>
    /// starts the server and begins accepting client connections asynchronously
    /// </summary>
    public async Task RunAsync()
    {
        Console.WriteLine("Server is ready, waiting for the players...");
        _listener.Start();

        _ = Task.Run(GameLoopAsync);

        // connection acceptance loop
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            client.NoDelay = true;
            lock (_clients) _clients.Add(client);
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    /// <summary>
    /// handles the lifecycle of a specific client connection 
    /// </summary>
    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using var reader = new StreamReader(client.GetStream());
            string? nickname = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(nickname)) nickname = "Guest";

            var player = _gameLogic.CreatePlayer(nickname);
            _playerTargets.TryAdd(player.Id, player.Position);
            Console.WriteLine($"Player {nickname} entered the server.");

            // continuously read mouse coordinates
            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null) break;
                if (_gameLogic.WinnerId != null) continue;

                var parts = line.Split(';');
                if (parts.Length == 2)
                {
                    float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    if (_playerTargets.ContainsKey(player.Id))
                    {
                        _playerTargets[player.Id] = new Vector2(x, y);
                    }
                }
            }
        }
        catch
        {
        }
        finally
        {
            // cleanup
            lock (_clients) _clients.Remove(client);
        }
    }

    /// <summary>
    /// updates physics and broadcasts state to all clients
    /// </summary>
    private async Task GameLoopAsync()
    {
        while (true)
        {
            // update game physics
            _gameLogic.Update();

            // serialize state
            string json = JsonSerializer.Serialize(_gameLogic.State, _jsonOptions);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");

            // broadcast to all connected clients
            lock (_clients)
            {
                foreach (var c in _clients)
                {
                    try { c.GetStream().Write(data, 0, data.Length); } catch { }
                }
            }
            await Task.Delay(20);
        }
    }
}