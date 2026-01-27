namespace AgarGame.Shared;
/// <summary>
/// container for constant values used across both Server and Client 
/// </summary>
public static class GameConstants
{
    public const int ScreenWidth = 800;
    public const int ScreenHeight = 600;
    public const int MapWidth = 2000;
    public const int MapHeight = 1500;
    public const string Host = "127.0.0.1";
    public const int Port = 6000;
    public const float StartSize = 40.0f;
    public const float WinSize = 500.0f;
    public const float MinAgility = 0.01f;
    public const float MaxAgility = 0.15f;
}