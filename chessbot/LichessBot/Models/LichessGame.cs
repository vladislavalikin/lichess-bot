using System.Text.Json;
using ServiceStack;

using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;

namespace chessbot.LichessBot.Models;


public class LichessGame
{
    private LichessConnection Connection;
    public string GameId { get; }
    public Game Game { get; }


    public delegate void gameStateEvent(LichessGame game, GameStateEvent e);
    public event gameStateEvent OnGameState;

    private int pos = 0;
    private List<string> blackMoves = new List<string> { "h7h6", "g7g6", "f7f6", "e7e6", "d7d6", "c7c6", "b7b6", "a7a6" };
    private List<string> whiteMoves = new List<string> { "h2h3", "g2g3", "f2f3", "e2e3", "d2d3", "c2c3", "b2b3", "a2a3" };

    public LichessGame(LichessConnection connection, Game game)
    {
        Connection = connection;
        Game = game;

        GameLoop();
    }

    private async void GameLoop()
    {
        Console.WriteLine("Game started");
        var url = $"https://lichess.org/api/bot/game/stream/{Game.id}";
        using var streamReader = new StreamReader(await Connection.HttpClient.GetStreamAsync(url));
        while (!streamReader.EndOfStream)
        {
            var message = await streamReader.ReadLineAsync();
            Console.WriteLine($"Game message: {message}");
            if (message.IsEmpty())
                continue;
            var eventType = JsonSerializer.Deserialize<LCStreamEvent>(message);
            switch (eventType.type)
            {
                case "gameFull":
                    var GameFull = JsonSerializer.Deserialize<GameFullEvent>(message);
                    break;
                case "gameState":
                    var gameState = JsonSerializer.Deserialize<GameStateEvent>(message);
                    OnGameState?.Invoke(this, gameState);
                    OnGameStateChanged(this, gameState);
                    break;
                case "opponentGone":
                    var OpponentGone = JsonSerializer.Deserialize<OpponentGoneEvent>(message);
                    break;
                case "chatLine":
                    var ChatLine = JsonSerializer.Deserialize<ChatLineEvent>(message);
                    break;
            }
        }
    }

    public async Task<bool> MakeMove(string gameId, string move)
    {
        var request = new HttpRequestMessage();
        request.RequestUri = Connection.GetUriBuilder($"https://lichess.org/api/bot/game/{gameId}/move/{move}").Uri;
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async void OnGameStarted(LichessGame lcGame)
    {
        if (lcGame.Game.isMyTurn)
        {
            await lcGame.MakeMove(lcGame.Game.id, lcGame.Game.color == "white" ? whiteMoves[pos++] : blackMoves[pos++]);
        }
    }

    public async void OnGameStateChanged(LichessGame lcGame, GameStateEvent e)
    {
        var color = lcGame.Game.color;
        var turns = e.moves.Split(' ').ToList();
        var turnColor = (turns.Count % 2) == 0 ? "white" : "black"; // "e2e4" -> 
        if (turnColor == lcGame.Game.color)
            await lcGame.MakeMove(lcGame.Game.id, turnColor == "white" ? whiteMoves[pos++] : blackMoves[pos++]);
    }
}
