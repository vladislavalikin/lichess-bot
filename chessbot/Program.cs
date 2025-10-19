using ServiceStack;
using System.Net.Http.Headers;
using System.Text.Json;
using LichessNET.API;
using chessbot.LichessBot.Models.StreamEventModels;
using chessbot.LichessBot;
using chessbot.LichessBot.Models;


Console.WriteLine("Hello, World!");

var lichessBot = new LichessBot(bearer: "lip_1zLfgbViMozLx9vwBSSd");

//var _httpClient = new HttpClient();
//_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "lip_1zLfgbViMozLx9vwBSSd");
//var url = "https://lichess.org/api/stream/event";

lichessBot.OnChallanged += LichessBot_OnChallanged;
lichessBot.OnGameStarted += LichessBot_OnGameStarted;

void LichessBot_OnGameStarted(LCGameStartedEvent e)
{
    throw new NotImplementedException();
}

async void LichessBot_OnChallanged(LCChallangeEvent e)
{
    var opponentRating = e.challenge.challenger.rating;
    if (opponentRating > 1500)
    {
        if (e.challenge.challenger.name == "VladislavAlikin")
        {
            var isAccept = await lichessBot.AcceptChallangeAsync(e.challenge.id);
            return;
        }
    }

    Console.WriteLine($"Rat({e.challenge.challenger.name}) is declined: ");
    var isDeclined = await lichessBot.DeclineChallageAsync(e.challenge.id);
}

while (true)
{
    try
    {
        //Console.WriteLine("Establishing connection");
        //using var streamReader = new StreamReader(await _httpClient.GetStreamAsync(url));
        //while (!streamReader.EndOfStream)
        //{
        //    var message = await streamReader.ReadLineAsync();
        //    Console.WriteLine($"Received message: {message}");
        //    if (message.IsEmpty())
        //        continue;
        //    var eventType = JsonSerializer.Deserialize<LCStreamEvent>(message);
        //    switch (eventType.type)
        //    {
        //        case "gameStart": 
        //            var Gamestarted = JsonSerializer.Deserialize<LCGameStartedEvent>(message);
        //            break;
        //        case "gameFinish": 
        //            var gamefinished = JsonSerializer.Deserialize<LCGameFinishedEvent>(message);
        //            break;
        //        case "challenge": 
        //            var challange = JsonSerializer.Deserialize<LCChallangeEvent>(message);
        //            break;
        //        case "challengeDeclined": 
        //            var Declinedchallange = JsonSerializer.Deserialize<LCChallangeDeclinedEvent>(message);
        //            break;
        //        case "challengeCanceled":
        //            var challangeCanceled = JsonSerializer.Deserialize<LCChallangeCanceledEvent>(message);
        //            break;
        //    }
        //}
    }
    catch (Exception ex)
    {
        //Here you can check for 
        //specific types of errors before continuing
        //Since this is a simple example, i'm always going to retry
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("Retrying in 5 seconds");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

return 0;


//var client = new LichessApiClient();
//await client.SetToken("lip_1zLfgbViMozLx9vwBSSd"); //lip_UP4lpqlR5W5ArbM2EPmT

//do
//{
//    await Task.Delay(3000);
//    Console.WriteLine("Request");
//    //Thread.Sleep(5000);
//    var challanges = await client.GetChallengesAsync();
//    Console.WriteLine("1");
//    if (challanges.In.Count == 0)
//        continue;
//    foreach (var challange in challanges.In)
//    {
//       if (!await client.AcceptChallengeAsync(challange.Id))
//            continue;
//        var gameId = challange.Id;
//        var game = new ChessGame();
//        await game.PlayGame(client, gameId);
//    }
//}
//while (1 == 1);

//public class ChessGame
//{
//    public async Task<Task> PlayGame(LichessApiClient client, string gameId)
//    {
//        await Task.Delay(1000);
//        List<string> moves = new List<string>() { "a2a3", "b2b3", "c2c3", "d2d3", "e2e3", "f2f3", "g2g3", "h2h3", };
//        int movePos = 0;
//        Console.WriteLine($"Game '{gameId}' has been started.");
//        var gameState = await client.GetGameState(gameId);
//        Console.WriteLine($"  Game state is '{gameState}'");

//        var gameStream = await client.GetGameStreamAsync(gameId);
//        gameStream.OnMoveMade += (sender, move) =>
//        {
//            Console.WriteLine($"Last move: {move.Notation}");
//        }
//        ;

//        while (gameState.Status == "started")
//        {
//            await Task.Delay(1000);
//            var move = moves[movePos++];
//            if (movePos == 8)
//                return Task.CompletedTask;

//            if (gameState.Moves.Split(" ").Count() % 2 == 0)
//                continue;

//            Console.WriteLine($"We will do a move {move}");
//            if (!await client.MakeMove(gameId, move))
//            {
//                await Task.Delay(200);
//                Console.WriteLine($"Impossible move: '{move}'");
//            }

//            else
//                Console.WriteLine($"We made a move '{move}'!");

//            await Task.Delay(200);

//            Console.WriteLine($"  Game state is '{gameState}'");
//            gameState = await client.GetGameState(gameId);
//        }

//        return Task.CompletedTask;
//    }
//}
