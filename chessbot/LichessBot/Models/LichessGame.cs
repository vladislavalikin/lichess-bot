using System.Text.Json;
using ServiceStack;

using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace chessbot.LichessBot.Models;


public class LichessGame
{
    private LichessConnection Connection;
    public string GameId { get; }
    public Game Game { get; }

    private Process engine;
    public List<string> Moves = new List<string>();

    public delegate void gameStateEvent(LichessGame game, GameStateEvent e);
    public event gameStateEvent OnGameState;

    private int pos = 0;
    private List<string> blackMoves = new List<string> { "h7h6", "g7g6", "f7f6", "e7e6", "d7d6", "c7c6", "b7b6", "a7a6" };
    private List<string> whiteMoves = new List<string> { "h2h3", "g2g3", "f2f3", "e2e3", "d2d3", "c2c3", "b2b3", "a2a3" };

    public LichessGame(LichessConnection connection, Game game)
    {
        Connection = connection;
        Game = game;
        StartEngine();
        GameLoop();
    }

    public async void StartEngine()
    {
        const string TheProgram = @"SOS-51_Arena.exe";
        engine = new Process();
        var psi = new ProcessStartInfo(TheProgram);
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardOutput = true;
        engine.StartInfo = psi;
        Console.WriteLine("Executing " + TheProgram);
        engine.Start();
        await InitNewEngineGame(engine);
    }

    public async Task<string> GetBestMove(GameStateEvent gameState)
    {
        engine.StandardInput.WriteLine($"position stratpos moves {gameState.moves} ");
        engine.StandardInput.WriteLine("isready");
        var line = "";

        while (!(line = await engine.StandardOutput.ReadLineAsync())?.Contains("readyok") ?? false) { }

        engine.StandardInput.WriteLine($"go wtime {gameState.wtime} btime {gameState.btime} winc {gameState.winc} binc {gameState.binc}");

        var bestmove = "";
        while (!(line = await engine.StandardOutput.ReadLineAsync())?.Contains("bestmove") ?? false)
        {
            bestmove = line.Split(' ')?[1]; // bestmove e2e4 ponder g1f1
        }

        return bestmove;
    }



    private static async Task InitNewEngineGame(Process p)
    {
        p.StandardInput.WriteLine("uci");
        while (await p.StandardOutput.ReadLineAsync() != "uciok") { }
        p.StandardInput.WriteLine("ucinewgame");
    }

    private async void GameLoop()
    {
        Console.WriteLine("Game started");
        var url = $"https://lichess.org/api/bot/game/stream/{Game.id}";
        using var streamReader = new StreamReader(await Connection.HttpClient.GetStreamAsync(url));
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
                    OnGameStateChanged(this, gameState);
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
        request.RequestUri = Connection.GetUriBuilder($"https://lichess.org/api/bot/game/{gameId}/move/{move}").Uri;
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async void OnGameStarted(LichessGame lcGame)
    {
        if (lcGame.Game.isMyTurn)
        {
            var bestmove = await GetBestMove(new GameStateEvent());
            //await lcGame.MakeMove(lcGame.Game.id, lcGame.Game.color == "white" ? whiteMoves[pos++] : blackMoves[pos++]);
        }
    }

    public async void OnGameStateChanged(LichessGame lcGame, GameStateEvent e)
    {
        var color = lcGame.Game.color;
        var turns = e.moves.Split(' ').ToList();
        var turnColor = (turns.Count % 2) == 0 ? "white" : "black"; // "e2e4" -> 
        if (turnColor == lcGame.Game.color)
            await lcGame.MakeMove(lcGame.Game.id, turnColor == "white" ? whiteMoves[pos++] : blackMoves[pos++]);
    }
}
