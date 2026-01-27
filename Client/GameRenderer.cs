using System.Numerics;
using Raylib_cs;
using AgarGame.Shared;

namespace AgarGame.Client;

/// <summary>
/// responsible for the visual presentation of the game
/// </summary>
public class GameRenderer
{
    public Camera2D Camera = new() { Zoom = 1.0f };

    private readonly GameState _state;
    private readonly string _myNick;

    private bool _isGameOver = false;
    private string _winnerText = "";

    public GameRenderer(GameState state, string myNick)
    {
        _state = state;
        _myNick = myNick;
    }

    /// <summary>
    /// checks the game state for the "Game Over" signal sent by the server
    /// done before drawing to determine which screen to show
    /// </summary>
    public void CheckGameOver()
    {
        lock (_state)
        {
            if (_state.Food.Count > 0 && _state.Food[0].X == -9999)
            {
                if (!_isGameOver)
                {
                    _isGameOver = true;
                    var winner = _state.Players.OrderByDescending(p => p.Size).FirstOrDefault();
                    if (winner != null)
                    {
                        _winnerText = $"{winner.Name} is a winner!";
                    }
                    else
                    {
                        _winnerText = "Game Over";
                    }
                }
            }
        }
    }
    /// <summary>
    /// main rendering pipeline
    /// </summary>
    public void Draw()
    {
        UpdateCamera();

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.RayWhite);

        if (!_isGameOver)
        {
            Raylib.BeginMode2D(Camera);
            DrawMap();
            DrawEntities();
            Raylib.EndMode2D();
            DrawLeaderboard();
        }
        else
        {
            DrawGameOverScreen();
        }

        Raylib.EndDrawing();
    }

    /// <summary>
    /// updates camera position and zoom level using Linear Interpolation 
    /// </summary>
    private void UpdateCamera()
    {
        if (_isGameOver) return;

        PlayerDto? me = null;
        float minDist = float.MaxValue;

        lock (_state)
        {
            foreach (var p in _state.Players)
            {
                if (p.Name == _myNick) { me = p; break; }
            }
            if (me == null)
            {
                foreach (var p in _state.Players)
                {
                    float d = Vector2.Distance(p.Position, Camera.Target);
                    if (d < minDist) { minDist = d; me = p; }
                }
            }
        }

        if (me != null)
        {
            Camera.Target = Vector2.Lerp(Camera.Target, me.Position, 0.05f);
            Camera.Offset = new Vector2(GameConstants.ScreenWidth / 2, GameConstants.ScreenHeight / 2);
            float targetZoom = 1.0f / (me.Size / 40.0f);
            if (targetZoom < 0.4f) targetZoom = 0.4f;
            if (targetZoom > 1.0f) targetZoom = 1.0f;
            Camera.Zoom = Camera.Zoom * 0.95f + targetZoom * 0.05f;
        }
    }

    private void DrawMap()
    {
        Raylib.DrawRectangleLines(0, 0, GameConstants.MapWidth, GameConstants.MapHeight, Color.DarkGray);
        for (int x = 0; x < GameConstants.MapWidth; x += 200) Raylib.DrawLine(x, 0, x, GameConstants.MapHeight, new Color(220, 220, 220, 255));
        for (int y = 0; y < GameConstants.MapHeight; y += 200) Raylib.DrawLine(0, y, GameConstants.MapWidth, y, new Color(220, 220, 220, 255));
    }

    private void DrawEntities()
    {
        lock (_state)
        {
            foreach (var f in _state.Food)
                if (f.X != -9999) Raylib.DrawCircleV(f, 6, GetFoodColor(f));

            foreach (var p in _state.Players)
            {
                Color c = new(p.ColorR, p.ColorG, p.ColorB, (byte)255);
                Raylib.DrawCircleV(p.Position, p.Size, c);
                Raylib.DrawCircleLines((int)p.Position.X, (int)p.Position.Y, p.Size, Color.Black);

                int fontSize = (int)(p.Size / 2);
                if (fontSize < 12) fontSize = 12;
                if (fontSize > 40) fontSize = 40;

                int nameW = Raylib.MeasureText(p.Name, fontSize);
                Raylib.DrawText(p.Name, (int)p.Position.X - nameW / 2, (int)p.Position.Y - fontSize / 2, fontSize, Color.Black);

                Raylib.DrawText($"{(int)p.Size}", (int)p.Position.X - 10, (int)p.Position.Y + fontSize / 2, 10, Color.DarkGray);
            }
        }
    }

    private void DrawLeaderboard()
    {
        int bx = GameConstants.ScreenWidth - 210;
        Raylib.DrawRectangle(bx, 10, 200, 150, new Color(0, 0, 0, 100));
        Raylib.DrawText("Leader Board", bx + 40, 20, 20, Color.White);

        lock (_state)
        {
            var leaders = _state.Players.OrderByDescending(p => p.Size).Take(5).ToList();
            for (int i = 0; i < leaders.Count; i++) Raylib.DrawText($"{i + 1}. {leaders[i].Name} - {(int)leaders[i].Size}", bx + 10, 50 + (i * 20), 18, Color.White);
        }
    }

    private void DrawGameOverScreen()
    {
        Raylib.ClearBackground(Color.Black);
        int wWidth = Raylib.MeasureText(_winnerText, 50);
        Raylib.DrawText(_winnerText, GameConstants.ScreenWidth / 2 - wWidth / 2, GameConstants.ScreenHeight / 2 - 50, 50, Color.Gold);

        string sub = "Press ESC to exit";
        int sWidth = Raylib.MeasureText(sub, 20);
        Raylib.DrawText(sub, GameConstants.ScreenWidth / 2 - sWidth / 2, GameConstants.ScreenHeight / 2 + 20, 20, Color.Gray);
    }

    private static Color GetFoodColor(Vector2 pos)
    {
        int hash = (int)(pos.X * 1234 + pos.Y * 5678);
        Random r = new(hash);
        return new Color(r.Next(50, 255), r.Next(50, 255), r.Next(50, 255), 255);
    }
}