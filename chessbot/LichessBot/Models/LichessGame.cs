using chessbot.LichessBot.Models.GameEventModels;
using chessbot.LichessBot.Models.StreamEventModels;
using ServiceStack;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
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

    private Process engineSpike;
    private Process engineRybka;
   
    public delegate void gameStateEvent(LichessGame game, GameStateEvent e);
    public event gameStateEvent OnGameState;

    public LichessGame(LichessConnection connection, Game game)
    {
        Connection = connection;
        Game = game;

        StartEngine();
        GameLoop();
    }

    public void StartEngine()
    {
        const string TheSpikeProgram = @"Spike1.4.exe";
        const string TheRybkaProgram = @"Rybkav2.3.2a.mp.x64.exe";
        engineSpike = new Process();
        engineRybka = new Process();
        var spsi = new ProcessStartInfo(TheSpikeProgram);
        var rpsi = new ProcessStartInfo(TheRybkaProgram);
        spsi.UseShellExecute = false;
        spsi.CreateNoWindow = true;
        spsi.RedirectStandardInput = true;
        spsi.RedirectStandardOutput = true;
        rpsi.UseShellExecute = false;
        rpsi.CreateNoWindow = true;
        rpsi.RedirectStandardInput = true;
        rpsi.RedirectStandardOutput = true;
        engineSpike.StartInfo = spsi;
        engineRybka.StartInfo = rpsi;
        Console.WriteLine("Executing " + TheSpikeProgram);
        Console.WriteLine("Executing " + TheRybkaProgram);
        engineSpike.Start();
        engineRybka.Start();
        InitNewEngineGame(engineSpike, engineRybka);
    }

    public async Task<string> GetBestMove(GameStateEvent gameState)
    {
        var actualwtime = gameState.wtime > 100000 ? 100000 : gameState.wtime;
        var actualbtime = gameState.btime > 100000 ? 100000 : gameState.btime;
        var actualwinc = gameState.winc > 100000 ? 100000 : gameState.winc;
        var actualbinc = gameState.binc > 100000 ? 100000 : gameState.binc;

        var line = "";
        var bestmove = "";
        // e2e4 e5e6 ...
        var movesCount = gameState.moves?.Split(' ').Count();
        
        if (movesCount <= 7)
        {
            // opening, play Spike.
            await engineSpike.StandardInput.WriteLineAsync($"position startpos moves {gameState.moves ?? ""} ");
            await engineSpike.StandardInput.WriteLineAsync($"go wtime {actualwtime} btime {actualbtime} winc {actualwinc} binc {actualbinc}");

            do
            {
                line = await engineSpike.StandardOutput.ReadLineAsync();
                if (line?.Contains("bestmove") ?? false)
                    bestmove = line.Split(' ')?[1]; // bestmove e2e4 ponder g1f1

            }
            while (!line?.Contains("bestmove") ?? true);
        }
        else
        {
            // rybka play then, spike play opening
            await engineRybka.StandardInput.WriteLineAsync($"position startpos moves {gameState.moves ?? ""} ");
            await engineRybka.StandardInput.WriteLineAsync($"go wtime {actualwtime} btime {actualbtime} winc {actualwinc} binc {actualbinc}");

            do
            {
                line = await engineRybka.StandardOutput.ReadLineAsync();
                if (line?.Contains("bestmove") ?? false)
                    bestmove = line.Split(' ')?[1]; // bestmove e2e4 ponder g1f1

            }
            while (!line?.Contains("bestmove") ?? true);
        }
        return bestmove ?? "";
    }


    private void InitNewEngineGame(Process sp, Process rp)
    {
        // starting spike engine
        sp.StandardInput.WriteLine("uci");
        while (sp.StandardOutput.ReadLine() != "uciok") { }

        sp.StandardInput.WriteLine("isready");
        while (sp.StandardOutput.ReadLine() != "readyok") { }

        sp.StandardInput.WriteLine("ucinewgame");

        sp.StandardInput.WriteLine("isready");
        while (sp.StandardOutput.ReadLine() != "readyok") { }

        // starting rybka engine

        rp.StandardInput.WriteLine("uci");
        while (rp.StandardOutput.ReadLine() != "uciok") { }

        rp.StandardInput.WriteLine("isready");
        while (rp.StandardOutput.ReadLine() != "readyok") { }

        rp.StandardInput.WriteLine("ucinewgame");

        rp.StandardInput.WriteLine("isready");
        while (rp.StandardOutput.ReadLine() != "readyok") { }

    }

    public void FinishGame()
    {
        engineSpike.Kill();
        engineSpike.Close();
        engineRybka.Kill();
        engineRybka.Close();
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
            if (message is null)
            {
                Console.WriteLine("Game loop message is null");
                continue;
            }
            Console.WriteLine($"Game message: {message}");
            if (message.IsEmpty())
                continue;
            var eventType = JsonSerializer.Deserialize<LCStreamEvent>(message) ?? new LCStreamEvent();
            switch (eventType.type)
            {
                case "gameFull":
                    var GameFull = JsonSerializer.Deserialize<GameFullEvent>(message) ?? new GameFullEvent();
                    wtime = GameFull.state.wtime;
                    btime = GameFull.state.btime;
                    winc = GameFull.state.winc;
                    binc = GameFull.state.binc;

                    break;
                case "gameState":
                    var gameState = JsonSerializer.Deserialize<GameStateEvent>(message) ?? new GameStateEvent();
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
        var response = await Connection.SendRequestAsync(request, HttpMethod.Post, content: null);
        return response.Content.ReadAsStringAsync().Result.Contains("true");
    }

    public async Task OnGameStarted(LichessGame lcGame)
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
