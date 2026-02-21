using chessbot.LichessBot;

Console.WriteLine("Lichess BOT started");

var LICHESS_BOT_TOKEN = Environment.GetEnvironmentVariable("LICHESS_BOT_TOKEN") ?? "";
var lichessBot = new LichessBot(bearer: LICHESS_BOT_TOKEN);

while (true) { }

