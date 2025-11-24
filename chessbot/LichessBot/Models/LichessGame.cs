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

    //private Process engineSpike;
    //private Process engineRybka;
    private Process engineKozachka;
    
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
        const string TheProgram = @"Uralochka3.40a-sse.exe";
        engineKozachka = new Process();
        var psi = new ProcessStartInfo(TheProgram);
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardOutput = true;
        engineKozachka.StartInfo = psi;
        Console.WriteLine("Executing" + TheProgram);
        engineKozachka.Start();
        InitNewEngineGame(engineKozachka);
    }

    public async Task<string> GetBestMove(GameStateEvent gameState)
    {
        if (engineKozachka.HasExited == true)
            return null;
        var actualwtime = gameState.wtime > 100000 ? 100000 : gameState.wtime;
        var actualbtime = gameState.btime > 100000 ? 100000 : gameState.btime;
        var actualwinc = gameState.winc > 100000 ? 100000 : gameState.winc;
        var actualbinc = gameState.binc > 100000 ? 100000 : gameState.binc;

        var line = "";
        var bestmove = "";
        // e2e4 e5e6 ...
        var movesCount = gameState.moves?.Split(' ').Count();
        
        // opening, play Spike.
        await engineKozachka.StandardInput.WriteLineAsync($"position startpos moves {gameState.moves ?? ""} ");
        await engineKozachka.StandardInput.WriteLineAsync($"go wtime {actualwtime} btime {actualbtime} winc {actualwinc} binc {actualbinc}");

        do
        {
            line = await engineKozachka.StandardOutput.ReadLineAsync();
            if (line?.Contains("bestmove") ?? false)
                bestmove = line.Split(' ')?[1]; // bestmove e2e4 ponder g1f1

        }
        while (!line?.Contains("bestmove") ?? true);
        return bestmove;
    }


    private void InitNewEngineGame(Process up)
    {
        // starting uralochka engine
        up.StandardInput.WriteLine("uci");
        while (up.StandardOutput.ReadLine() != "uciok") { }

        up.StandardInput.WriteLine("isready");
        while (up.StandardOutput.ReadLine() != "readyok") { }

        up.StandardInput.WriteLine("ucinewgame");

        up.StandardInput.WriteLine("isready");
        while (up.StandardOutput.ReadLine() != "readyok") { }
    }

    public void FinishGame()
    {
        engineKozachka.Kill();
        engineKozachka.Close();
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

                    var gs = new GameStateEvent
                    {
                        moves = GameFull.state.moves,
                        wtime = GameFull.state.wtime,
                        btime = GameFull.state.btime,
                        winc = GameFull.state.winc,
                        binc = GameFull.state.binc
                    };
                    await OnGameStateChanged(this, gs);

                    break;
                case "gameState":
                    var gameState = JsonSerializer.Deserialize<GameStateEvent>(message) ?? new GameStateEvent();
                    if (gameState.status != "started")
                        break;
                    OnGameState?.Invoke(this, gameState);
                    await OnGameStateChanged(this, gameState);
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

    public async Task OnGameStateChanged(LichessGame lcGame, GameStateEvent e)
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
