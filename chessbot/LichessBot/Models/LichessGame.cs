using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;
using ServiceStack;

namespace chessbot.LichessBot.Models
{

    public class LichessGame
    {
        private HttpClient HttpClient;
        public string GameId { get; }

        public LichessGame(HttpClient httpClient, string gameId)
        {
            HttpClient = httpClient;
            GameId = gameId;

            MainLoop();
        }

        private async void MainLoop()
        {
            Console.WriteLine("Game started");
            var url = $"https://lichess.org/api/stream/{GameId}";
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
                    case "gameFullEvent":
                        var GameFull = JsonSerializer.Deserialize<GameFullEvent>(message);
                        break;
                    case "gameStateEvent":
                        var gameState = JsonSerializer.Deserialize<GameStateEvent>(message);
                        break;
                    case "OpponentGoneEvent":
                        var OpponentGone = JsonSerializer.Deserialize<OpponentGoneEvent>(message);
                        break;
                    case "ChatLineEvent":
                        var ChatLine = JsonSerializer.Deserialize<ChatLineEvent>(message);
                        break;
                }
            }
        }
    }
}
