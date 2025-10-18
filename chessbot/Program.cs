// See https://aka.ms/new-console-template for more information
using LichessNET.API;
using LichessNET.Entities.Game;
using LichessNET.Entities.Social;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using static System.Net.WebRequestMethods;

var _httpClient = new HttpClient();


Console.WriteLine("Hello, World!");


_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "lip_1zLfgbViMozLx9vwBSSd");
var url = "https://lichess.org/api/stream/event";

while (true)
{
    try
    {
        Console.WriteLine("Establishing connection");
        using var streamReader = new StreamReader(await _httpClient.GetStreamAsync(url));
        while (!streamReader.EndOfStream)
        {
            var message = await streamReader.ReadLineAsync();
            Console.WriteLine($"Received price update: {message}");
        }
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
//        if (!await client.AcceptChallengeAsync(challange.Id))
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
    