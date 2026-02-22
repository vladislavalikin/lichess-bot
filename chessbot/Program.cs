using chessbot.LichessBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

Console.WriteLine("Lichess BOT started");
var configFile = "chessbot.txt";

if (!File.Exists(configFile))
{
    Console.WriteLine($"File '{configFile}' with your token doesnt exists, first download this file");
    Console.ReadKey();
    return;
}

var settings = File.ReadAllLines(configFile)
    .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains('='))
    .Select(line => line.Split('=', 2))
    .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
if (settings.TryGetValue("Token", out string token))
{ 
    var lichessBot = new LichessBot(bearer: token);
}

while (true) { }

