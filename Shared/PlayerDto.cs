using System.Numerics;
using System.Text.Json.Serialization;

namespace AgarGame.Shared;
/// <summary>
/// DTO representing a single player 
/// </summary>
public class PlayerDto
{
    public string Id { get; set; } = "";
    [JsonPropertyName("n")]
    public string Name { get; set; } = "Player";
    [JsonPropertyName("p")]
    public Vector2 Position { get; set; }
    [JsonPropertyName("s")]
    public float Size { get; set; }
    public byte ColorR { get; set; }
    public byte ColorG { get; set; }
    public byte ColorB { get; set; }
}