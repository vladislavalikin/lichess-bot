using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using ServiceStack;

using chessbot.LichessBot.Models.StreamEventModels;
using chessbot.LichessBot.Models;
using LichessNET.Entities.Game;
using chessbot.LichessBot.Models.GameEventModels;


namespace chessbot.LichessBot;

public class LichessBot
{
    private const string BaseUrl = "https://lichess.org/";

    private HttpClient _httpClient = new HttpClient();
    
    public delegate void ChallengeEvent(LCChallangeEvent e); 
    public event ChallengeEvent OnChallanged;

    public delegate void OnGameStartedEvent(LCGameStartedEvent e);
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
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        MainLoop();
    }

    private async void MainLoop()
    {
        Console.WriteLine("Establishing connection");
        var url = "https://lichess.org/api/stream/event";
        using var streamReader = new StreamReader(await _httpClient.GetStreamAsync(url));
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
                    OnGameStarted?.Invoke(gameStarted);
                    var newGame = new LichessGame(_httpClient, gameStarted.game);
                    Games.Add(newGame);
                    newGame.OnGameState += NewGame_OnGameState;
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
        request.RequestUri = GetUriBuilder($"https://lichess.org/api/challenge/{challengeId}/accept").Uri;
        var response = await SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async Task<bool> DeclineChallageAsync(string challengeId)
    {
        var request = new HttpRequestMessage();
        request.RequestUri = GetUriBuilder($"https://lichess.org/api/challenge/{challengeId}/decline").Uri;
        var response = await SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public UriBuilder GetUriBuilder(string endpoint, params Tuple<string, string>[] queryParameters)
    {
        var builder = new UriBuilder(endpoint);
        builder.Port = -1;

        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach (var param in queryParameters) 
            query[param.Item1] = param.Item2;

        builder.Query = query.ToString();

        return builder;
    }

    public  async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, HttpMethod method = null,
    HttpContent content = null)
    {
        if (method == null) 
            method = HttpMethod.Get;
        var client = _httpClient;

        request.Method = method;
        request.Content = content;

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException("Access denied. Your token does not have the required scope.");
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Api Key is invalid.");
        }

        return response;
    }
}
