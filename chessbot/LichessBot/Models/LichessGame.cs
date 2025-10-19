using System.Text.Json;
using ServiceStack;

using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;


namespace chessbot.LichessBot.Models;

public class LichessGame
{
    private HttpClient HttpClient;
    public string GameId { get; }
    public Game Game { get; }


    public delegate void gameStateEvent(LichessGame game, GameStateEvent e);
    public event gameStateEvent OnGameState;

    public LichessGame(HttpClient httpClient, Game game)
    {
        HttpClient = httpClient;
        Game = game;

        GameLoop();
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
                    OnGameState?.Invoke(this, gameState);
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
        request.RequestUri = GetUriBuilder($"https://lichess.org/api/bot/game/{gameId}/move/{move}").Uri;
        var response = await SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }
}
