using System.Numerics; 
using System.Collections.Generic;

namespace AgarGame.Shared; 

/// <summary>
/// complete snapshot of the game world at a specific moment, is serialized to JSON
/// </summary>
public class GameState
{
    public List<PlayerDto> Players { get; set; } = [];
    public List<Vector2> Food { get; set; } = [];
}