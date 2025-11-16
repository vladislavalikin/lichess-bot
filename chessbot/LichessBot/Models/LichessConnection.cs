using System.Net;
using System.Net.Http.Headers;
using System.Web;


namespace chessbot.LichessBot.Models;

public class LichessConnection
{
    private const string BaseUrl = "https://lichess.org/";

    public HttpClient HttpClient { get; set; } = new HttpClient();

    public LichessConnection(string bearer) 
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
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

    public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, HttpMethod method, HttpContent content)
    {
        if (method == null)
            method = HttpMethod.Get;

        request.Method = method;
        request.Content = content;

        var response = await HttpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return response;

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new HttpRequestException("Access denied. Your token does not have the required scope.");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Api Key is invalid.");

        return response;
    }
}
