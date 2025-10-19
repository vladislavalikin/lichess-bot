using chessbot.LichessBot.Models.StreamEventModels;
using chessbot.LichessBot;
using chessbot.LichessBot.Models.GameEventModels;


Console.WriteLine("Hello, World!");

var blackMoves = new List<string> { "h7h6", "g7g6", "f7f6", "e7e6", "d7d6", "c7c6", "b7b6", "a7a6" };
var whiteMoves = new List<string> { "h2h3", "g2g3", "f2f3", "e2e3", "d2d3", "c2c3", "b2b3", "a2a3" };

var lichessBot = new LichessBot(bearer: "lip_1zLfgbViMozLx9vwBSSd");

var pos = 0;

lichessBot.OnChallanged += LichessBot_OnChallanged;
lichessBot.OnGameStarted += LichessBot_OnGameStarted;
lichessBot.OnGameState += LichessBot_OnGameStateChanged;

void LichessBot_OnGameStateChanged(chessbot.LichessBot.Models.LichessGame game, GameStateEvent e)
{
    var color = game.Game.color;
    var isWhiteMove = (e.moves.Length / 4) % 2; // "e2e4" -> 
    //lichessBot.MakeMove(game.GameId, );
}

void LichessBot_OnGameStarted(LCGameStartedEvent e)
{
    if (e.game.isMyTurn)
    {
        lichessBot.MakeMove(e.game.id, e.game.color == "white" ? whiteMoves[pos++] : blackMoves[pos++]);
    }
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

while (true) { }

