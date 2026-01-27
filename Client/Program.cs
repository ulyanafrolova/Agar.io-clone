using AgarGame.Client;
using AgarGame.Shared;
using Raylib_cs;

namespace AgarGame.Client;

/// <summary>
/// main entry point for the Client application
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        Console.Write("Enter your nickname: ");
        string? myNick = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(myNick)) myNick = "Player";

        Raylib.InitWindow(GameConstants.ScreenWidth, GameConstants.ScreenHeight, $"Agar.io - {myNick}");
        Raylib.SetTargetFPS(60);

        // network Initialization
        Console.WriteLine($"Connecting to {GameConstants.Host}...");
        var netClient = new NetworkClient(myNick);

        // if the server is offline, show a message 
        if (!netClient.Connect())
        {
            Console.WriteLine("Could not connect to server!");
            Console.WriteLine("Please ensure 'AgarGame.Server' is running first.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return; // exit application
        }

        // pass the Shared State from the NetworkClient to the Renderer
        var renderer = new GameRenderer(netClient.CurrentState, myNick);

        // main loop runs until the user clicks presses ESC or 'X' button
        while (!Raylib.WindowShouldClose())
        {
            netClient.SendInput(renderer.Camera);
            renderer.CheckGameOver();
            // draw the map, players, and food based on the latest state
            renderer.Draw();
        }

        // cleanup
        netClient.Disconnect();
        Raylib.CloseWindow();
    }
}