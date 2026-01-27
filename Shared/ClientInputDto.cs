using System.Text.Json.Serialization;

namespace AgarGame.Shared;
/// <summary>
/// DTO used to send input data from the Client to the Server
/// represents the player's desired direction 
/// </summary>
public class ClientInputDto
{
    [JsonPropertyName("x")]
    public float TargetX { get; set; }

    [JsonPropertyName("y")]
    public float TargetY { get; set; }
}
