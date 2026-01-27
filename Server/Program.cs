using AgarGame.Server;

namespace AgarGame.Server;

/// <summary>
/// the main entry point for the Server application
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Server starting...");
        // main server engine which encapsulates TCP listener and Game Logic.
        ServerEngine server = new();
        await server.RunAsync();
    }
}
