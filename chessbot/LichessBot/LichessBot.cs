using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using chessbot.LichessBot.Models.StreamEventModels;
using Microsoft.VisualBasic;
using ServiceStack;
using ServiceStack.Web;

namespace chessbot.LichessBot;

public class LichessBot
{
    private const string BaseUrl = "https://lichess.org/";

    private HttpClient _httpClient = new HttpClient();
    
    public delegate void ChallengeEvent(LCChallangeEvent e); 
    public event ChallengeEvent OnChallanged;

    public LichessBot(string bearer) 
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "lip_1zLfgbViMozLx9vwBSSd");
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
                    var Gamestarted = JsonSerializer.Deserialize<LCGameStartedEvent>(message);
                    break;
                case "gameFinish":
                    var gamefinished = JsonSerializer.Deserialize<LCGameFinishedEvent>(message);
                    break;
                case "challenge":
                    var challange = JsonSerializer.Deserialize<LCChallangeEvent>(message);
                    OnChallanged?.Invoke(challange);
                    break;
                case "challengeDeclined":
                    var Declinedchallange = JsonSerializer.Deserialize<LCChallangeDeclinedEvent>(message);
                    break;
                case "challengeCanceled":
                    var challangeCanceled = JsonSerializer.Deserialize<LCChallangeCanceledEvent>(message);
                    break;
            }
        }
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

    private UriBuilder GetUriBuilder(string endpoint, params Tuple<string, string>[] queryParameters)
    {
        var builder = new UriBuilder(endpoint);
        builder.Port = -1;

        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach (var param in queryParameters) query[param.Item1] = param.Item2;

        builder.Query = query.ToString();

        return builder;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, HttpMethod method = null,
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
