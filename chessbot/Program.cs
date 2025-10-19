using chessbot.LichessBot.Models.StreamEventModels;
using chessbot.LichessBot;


Console.WriteLine("Hello, World!");

var lichessBot = new LichessBot(bearer: "lip_1zLfgbViMozLx9vwBSSd");

lichessBot.OnChallanged += LichessBot_OnChallanged;
lichessBot.OnGameStarted += LichessBot_OnGameStarted;

void LichessBot_OnGameStarted(LCGameStartedEvent e)
{
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

