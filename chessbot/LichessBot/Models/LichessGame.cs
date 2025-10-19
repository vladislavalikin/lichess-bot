using System.Text.Json;
using ServiceStack;

using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;


namespace chessbot.LichessBot.Models;

public class LichessGame
{
    private HttpClient HttpClient;
    public string GameId { get; }

    public LichessGame(HttpClient httpClient, string gameId)
    {
        HttpClient = httpClient;
        GameId = gameId;

        GameLoop();
        //MoveLoop();
    }

    private async void GameLoop()
    {
        Console.WriteLine("Game started");
        var url = $"https://lichess.org/api/bot/game/stream/{GameId}";
        using var streamReader = new StreamReader(await HttpClient.GetStreamAsync(url));
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

    private async void MoveLoop()
    {
        Console.WriteLine("Game started");
        var url = $"https://lichess.org/api/stream/game/{GameId}";
        using var streamReader = new StreamReader(await HttpClient.GetStreamAsync(url));
        while (!streamReader.EndOfStream)
        {
            var message = await streamReader.ReadLineAsync();
            if (message.IsEmpty())
                continue;
            Console.WriteLine($"Game move: {message}");
            if (message.IsEmpty())
                continue;
            //var eventType = JsonSerializer.Deserialize<LCStreamEvent>(message);
        }
    }
}
