using System.Net.Sockets;
using System.Text.Json;
using System.Globalization;
using System.Numerics;
using Raylib_cs;
using AgarGame.Shared;

namespace AgarGame.Client;

/// <summary>
/// responsible for sending player input and receiving game state updates asynchronously
/// </summary>
public class NetworkClient
{
    public GameState CurrentState { get; private set; } = new();

    private readonly TcpClient _client = new();
    private readonly Lock _writeLock = new();
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private readonly string _nickname;

    public NetworkClient(string nickname)
    {
        _nickname = nickname;
    }

    /// <summary>
    /// establishes a connection to the server and starts the background listening thread
    /// </summary>
    /// <returns>true if connected successfully, otherwise false</returns>
    public bool Connect()
    {
        try
        {
            _client.Connect(GameConstants.Host, GameConstants.Port);
            _client.NoDelay = true;

            var stream = _client.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream) { AutoFlush = true };

            _writer.WriteLine(_nickname);

            // start the receiver loop in a separate background task
            _ = Task.Run(ReceiveLoop);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// continuously listens for GameState updates from the server
    /// runs on a background thread
    /// </summary>
    private void ReceiveLoop()
    {
        var opts = new JsonSerializerOptions { IncludeFields = true };
        while (true)
        {
            try
            {
                string? json = _reader?.ReadLine();
                if (json != null)
                {
                    var newState = JsonSerializer.Deserialize<GameState>(json, opts);
                    if (newState != null)
                    {
                        // Bezpieczna podmiana stanu (referencji)
                        lock (CurrentState)
                        {
                            CurrentState.Players = newState.Players;
                            CurrentState.Food = newState.Food;
                        }
                    }
                }
            }
            catch { break; }
        }
    }

    /// <summary>
    /// calculates mouse position in the game world and sends it to the server
    /// </summary>
    /// <param name="camera">current camera view for coordinate translation</param>
    public void SendInput(Camera2D camera)
    {
        if (_writer == null) return;

        Vector2 mouseScreen = Raylib.GetMousePosition();
        Vector2 mouseWorld = Raylib.GetScreenToWorld2D(mouseScreen, camera);
        lock (_writeLock)
        {
            try
            {
                _writer.WriteLine($"{mouseWorld.X.ToString(CultureInfo.InvariantCulture)};{mouseWorld.Y.ToString(CultureInfo.InvariantCulture)}");
            }
            catch { }
        }
    }

    public void Disconnect()
    {
        _client.Close();
    }
}