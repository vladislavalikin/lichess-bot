using System.Text.Json;
using ServiceStack;

using chessbot.LichessBot.Models.StreamEventModels;
using chessbot.LichessBot.Models;
using chessbot.LichessBot.Models.GameEventModels;


namespace chessbot.LichessBot;

public class LichessBot
{
    private LichessConnection Connection;
    
    public delegate void ChallengeEvent(LCChallangeEvent e); 
    public event ChallengeEvent OnChallanged;

    public delegate void OnGameStartedEvent(LichessGame lcGame);
    public event OnGameStartedEvent OnGameStarted;

    public delegate void OnGameFinishedEvent(LCGameFinishedEvent e);
    public event OnGameFinishedEvent OnGameFinished;

    public delegate void OnChallangeDeclinedEvent(LCChallangeDeclinedEvent e);
    public event OnChallangeDeclinedEvent OnChallangeDeclined;

    public delegate void OnChallangeCanceledEvent(LCChallangeCanceledEvent e);
    public event OnChallangeCanceledEvent OnChallangeCanceled;

    public delegate void gameStateEvent(LichessGame game, GameStateEvent e);
    public event gameStateEvent OnGameState;

    public List<LichessGame> Games { get; set; } = new List<LichessGame>();

    public LichessBot(string bearer) 
    {
        Connection = new LichessConnection(bearer);

        OnChallanged += LichessBot_OnChallanged;

        MainLoop();
    }

    private async void MainLoop()
    {
        var url = "https://lichess.org/api/stream/event";
        using var streamReader = new StreamReader(await Connection.HttpClient.GetStreamAsync(url));
        while (!streamReader.EndOfStream)
        {
            var message = await streamReader.ReadLineAsync();
            if (message == null)
            {
                Console.WriteLine("message is null");
                continue;
            }
            Console.WriteLine($"Received message: {message}");
            if (message.IsEmpty())
                continue;
            var eventType = JsonSerializer.Deserialize<LCStreamEvent>(message) ?? new LCStreamEvent();
            switch (eventType.type)
            {
                case "gameStart":
                    Console.WriteLine("gameStart event");
                    var gameStarted = JsonSerializer.Deserialize<LCGameStartedEvent>(message) ?? new LCGameStartedEvent();
                    var newGame = new LichessGame(Connection, gameStarted.game);
                    newGame.OnGameState += NewGame_OnGameState;
                    Games.Add(newGame);
                    await newGame.OnGameStarted(newGame);
                    OnGameStarted?.Invoke(newGame);
                    break;
                case "gameFinish":
                    Console.WriteLine("gameFinish event");
                    var gameFinished = JsonSerializer.Deserialize<LCGameFinishedEvent>(message) ?? new LCGameFinishedEvent();
                    var game = Games.FirstOrDefault(g => g.Game.id == gameFinished?.game.id);
                    game?.FinishGame();
                    if (game is not null) 
                    {
                        Games.Remove(game);
                        OnGameFinished?.Invoke(gameFinished);
                    }
                    break;
                case "challenge":
                    Console.WriteLine("challange event");
                    var challange = JsonSerializer.Deserialize<LCChallangeEvent>(message) ?? new LCChallangeEvent();
                    OnChallanged?.Invoke(challange);
                    break;
                case "challengeDeclined":
                    Console.WriteLine("challange declined event");
                    var Declinedchallange = JsonSerializer.Deserialize<LCChallangeDeclinedEvent>(message) ?? new LCChallangeDeclinedEvent();
                    OnChallangeDeclined?.Invoke(Declinedchallange);
                    break;
                case "challengeCanceled":
                    Console.WriteLine("challange cancelled event");
                    var challangeCanceled = JsonSerializer.Deserialize<LCChallangeCanceledEvent>(message) ?? new LCChallangeCanceledEvent();
                    OnChallangeCanceled?.Invoke(challangeCanceled);
                    break;
            }
        }
    }

    private void NewGame_OnGameState(LichessGame game, GameStateEvent e)
    {
        OnGameState?.Invoke(game, e);
    }

    public async Task<bool> AcceptChallangeAsync(string challengeId)
    {
        var request = new HttpRequestMessage();
        request.RequestUri = Connection.GetUriBuilder($"https://lichess.org/api/challenge/{challengeId}/accept").Uri;
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post, content: null);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async Task<bool> DeclineChallageAsync(string challengeId)
    {
        var request = new HttpRequestMessage();
        request.RequestUri = Connection.GetUriBuilder($"https://lichess.org/api/challenge/{challengeId}/decline").Uri;
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post, content: null);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async void LichessBot_OnChallanged(LCChallangeEvent e)
    {
        if (Games.Count >= 1)
            await DeclineChallageAsync(e.challenge.id);
        if (e.challenge.speed != "blitz" && e.challenge.speed != "bullet")
            await DeclineChallageAsync(e.challenge.id);

        await AcceptChallangeAsync(e.challenge.id);
    }
}
