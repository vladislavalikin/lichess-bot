using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;
using ServiceStack;
using System.Diagnostics;
using System.Text.Json;
using Game = chessbot.LichessBot.Models.StreamEventModels.Game;

namespace chessbot.LichessBot.Models;


public class LichessGame
{
    private LichessConnection Connection;
    //public string GameId { get; }
    public int wtime { get; set; } = 0;
    public int btime { get; set; } = 0; 
    public int winc { get; set; } = 0;
    public int binc { get; set; } = 0;

    public Game Game { get; }

    private Process engine;
    public List<string> Moves = new List<string>();

    public delegate void gameStateEvent(LichessGame game, GameStateEvent e);
    public event gameStateEvent OnGameState;

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
        InitNewEngineGame(engine);
    }

    public async Task<string> GetBestMove(GameStateEvent gameState)
    {
        var actualwtime = gameState.wtime > 100000 ? 100000 : gameState.wtime;
        var actualbtime = gameState.btime > 100000 ? 100000 : gameState.btime;
        var actualwinc = gameState.winc > 100000 ? 100000 : gameState.winc;
        var actualbinc = gameState.binc > 100000 ? 100000 : gameState.binc;
        await engine.StandardInput.WriteLineAsync($"position startpos moves {gameState.moves ?? ""} ");
        await engine.StandardInput.WriteLineAsync($"go wtime {actualwtime} btime {actualbtime} winc {actualwinc} binc {actualbinc}");
        var line = "";
        
        var bestmove = "";
        do
        {
            line = await engine.StandardOutput.ReadLineAsync();
            if (line?.Contains("bestmove") ?? false)
                bestmove = line.Split(' ')?[1]; // bestmove e2e4 ponder g1f1

        }
        while (!line?.Contains("bestmove") ?? true);

        return bestmove ?? "";
    }


    private void InitNewEngineGame(Process p)
    {
        p.StandardInput.WriteLine("uci");
        while (p.StandardOutput.ReadLine() != "uciok") { }

        p.StandardInput.WriteLine("isready");
        while (p.StandardOutput.ReadLine() != "readyok") { }

        p.StandardInput.WriteLine("ucinewgame");
    }

    public async void FinishGame()
    {
        engine.Close();
        Console.WriteLine("Game finished");
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
                    wtime = GameFull.state.wtime;
                    btime = GameFull.state.btime;
                    winc = GameFull.state.winc;
                    binc = GameFull.state.binc;

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
            var bestmove = await GetBestMove(new GameStateEvent() { wtime = 10000, btime = 10000, winc = 0, binc = 0 });
            if (!bestmove.IsEmpty())
                await lcGame.MakeMove(lcGame.Game.id, bestmove);
        }
    }

    public async void OnGameStateChanged(LichessGame lcGame, GameStateEvent e)
    {
        var color = lcGame.Game.color;
        var turns = e.moves.Split(' ').ToList();
        var turnColor = (turns.Count % 2) == 0 ? "white" : "black"; // "e2e4" -> 
        if (turnColor == lcGame.Game.color)
        {
            var bestMove = await GetBestMove(e);
            if (!bestMove.IsEmpty())
                await lcGame.MakeMove(lcGame.Game.id, bestMove);
        }
        //await lcGame.MakeMove(lcGame.Game.id, turnColor == "white" ? whiteMoves[pos++] : blackMoves[pos++]);
    }
}
