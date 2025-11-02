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
            Console.WriteLine($"Received message: {message}");
            if (message.IsEmpty())
                continue;
            var eventType = JsonSerializer.Deserialize<LCStreamEvent>(message);
            switch (eventType.type)
            {
                case "gameStart":
                    var gameStarted = JsonSerializer.Deserialize<LCGameStartedEvent>(message);
                    var newGame = new LichessGame(Connection, gameStarted.game);
                    newGame.OnGameState += NewGame_OnGameState;
                    Games.Add(newGame);
                    newGame.OnGameStarted(newGame);
                    OnGameStarted?.Invoke(newGame);
                    break;
                case "gameFinish":
                    var gameFinished = JsonSerializer.Deserialize<LCGameFinishedEvent>(message);
                    OnGameFinished?.Invoke(gameFinished);
                    break;
                case "challenge":
                    var challange = JsonSerializer.Deserialize<LCChallangeEvent>(message);
                    OnChallanged?.Invoke(challange);
                    break;
                case "challengeDeclined":
                    var Declinedchallange = JsonSerializer.Deserialize<LCChallangeDeclinedEvent>(message);
                    OnChallangeDeclined?.Invoke(Declinedchallange);
                    break;
                case "challengeCanceled":
                    var challangeCanceled = JsonSerializer.Deserialize<LCChallangeCanceledEvent>(message);
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
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async Task<bool> DeclineChallageAsync(string challengeId)
    {
        var request = new HttpRequestMessage();
        request.RequestUri = Connection.GetUriBuilder($"https://lichess.org/api/challenge/{challengeId}/decline").Uri;
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    async void LichessBot_OnChallanged(LCChallangeEvent e)
    {
        var opponentRating = e.challenge.challenger.rating;
        if (opponentRating > 1500)
        {
            //if (e.challenge.challenger.name == "VladislavAlikin")
            {
                var isAccept = await AcceptChallangeAsync(e.challenge.id);
                return;
            }
        }

        Console.WriteLine($"Rat({e.challenge.challenger.name}) is declined: ");
        var isDeclined = await DeclineChallageAsync(e.challenge.id);
    }
}
