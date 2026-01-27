using System.Numerics;
using System.Collections.Concurrent;
using AgarGame.Shared;

namespace AgarGame.Server;

/// <summary>
/// encapsulates the core game mechanics, physics, and rules
/// </summary>
public class GameLogic
{
    public GameState State { get; private set; } = new();
    public string? WinnerId { get; private set; } = null;

    private readonly Random _rnd = new();
    // Thread-safe dictionary storing where each player wants to go (input from network threads)
    private readonly ConcurrentDictionary<string, Vector2> _playerTargets;

    public GameLogic(ConcurrentDictionary<string, Vector2> playerTargets)
    {
        _playerTargets = playerTargets;
        for (int i = 0; i < 200; i++)
        {
            State.Food.Add(GenerateRandomPosition());
        }
    }

    /// <summary>
    /// creates a new player with random properties and adds them to the game state
    /// </summary>
    /// <param name="nickname">The player's chosen name.</param>
    /// <returns>created PlayerDto object</returns>
    public PlayerDto CreatePlayer(string nickname)
    {
        var player = new PlayerDto
        {
            Id = Guid.NewGuid().ToString().Substring(0, 5),
            Name = nickname,
            Position = new Vector2(_rnd.Next(200, 1800), _rnd.Next(200, 1300)),
            Size = GameConstants.StartSize,
            ColorR = (byte)_rnd.Next(50, 255),
            ColorG = (byte)_rnd.Next(50, 255),
            ColorB = (byte)_rnd.Next(50, 255)
        };

        lock (State) State.Players.Add(player);
        return player;
    }

    private Vector2 GenerateRandomPosition()
    {
        return new Vector2(_rnd.Next(0, 2000), _rnd.Next(0, 1500));
    }

    /// <summary>
    /// updates the game physics (movement, collisions, win conditions)
    /// should be called in the main game loop
    /// </summary>
    public void Update()
    {
        if (WinnerId != null) return;

        lock (State)
        {
            for (int i = 0; i < State.Players.Count; i++)
            {
                var p = State.Players[i];

                if (p.Size >= GameConstants.WinSize)
                {
                    WinnerId = p.Id;
                    State.Food.Clear();
                    State.Food.Add(new Vector2(-9999, -9999));
                }

                if (_playerTargets.TryGetValue(p.Id, out Vector2 targetPos))
                {
                    float agility = 4.0f / p.Size;
                    if (agility < GameConstants.MinAgility) agility = GameConstants.MinAgility;
                    if (agility > GameConstants.MaxAgility) agility = GameConstants.MaxAgility;

                    // interpolate current position towards target position
                    p.Position = Vector2.Lerp(p.Position, targetPos, agility);
                    p.Position = new Vector2(Math.Clamp(p.Position.X, 0, 2000), Math.Clamp(p.Position.Y, 0, 1500));
                }

                for (int f = State.Food.Count - 1; f >= 0; f--)
                {
                    if (State.Food[f].X == -9999) continue;
                    if (Vector2.Distance(p.Position, State.Food[f]) < p.Size)
                    {
                        State.Food.RemoveAt(f);
                        p.Size += 1.0f;
                        State.Food.Add(GenerateRandomPosition());
                    }
                }
            }

            // check every player against every other player
            for (int i = 0; i < State.Players.Count; i++)
            {
                for (int j = 0; j < State.Players.Count; j++)
                {
                    if (i == j) continue;
                    var hunter = State.Players[i];
                    var victim = State.Players[j];

                    // hunter must be at least 20% larger to eat the victim
                    if (hunter.Size > victim.Size * 1.2f && Vector2.Distance(hunter.Position, victim.Position) < hunter.Size)
                    {
                        hunter.Size += victim.Size * 0.25f;
                        victim.Size = 40;
                        // try to find a spawn point far away from the hunter to avoid spawn-killing
                        bool safe = false;
                        for (int k = 0; k < 15; k++)
                        {
                            Vector2 tryPos = new(_rnd.Next(200, 1800), _rnd.Next(200, 1300));
                            if (Vector2.Distance(tryPos, hunter.Position) > 500)
                            {
                                victim.Position = tryPos;
                                safe = true;
                                break;
                            }
                        }
                        // if no safe spot found
                        if (!safe) victim.Position = new Vector2(_rnd.Next(200, 1800), _rnd.Next(200, 1300));
                    }
                }
            }
        }
    }
}