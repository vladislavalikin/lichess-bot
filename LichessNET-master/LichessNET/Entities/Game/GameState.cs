using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;

//{
//"type": "gameState",
//"moves": "e2e4 c7c5 f2f4 d7d6 g1f3 b8c6 f1c4 g8f6 d2d3 g7g6 e1g1 f8g7 b1c3",
//"wtime": 7598040,
//"btime": 8395220,
//"winc": 10000,
//"binc": 10000,
//"wdraw": false,
//"bdraw": false,
//"wtakeback": false,
//"btakeback": false,
//"status": "started"
//}

namespace LichessNET.Entities.Game
{
    public class GameState
    {
        public string Type { get; set; }
        public string Moves { get; set; }
        public int Wtime { get; set; }
        public int Btime { get; set; }
        public int Winc { get; set; }
        public int Binc { get; set; }
        public bool Wdraw { get; set; }
        public bool Bdraw { get; set; }
        public bool Wtakeback { get; set; }
        public bool Btakeback { get; set; }
        public string Status { get; set; }
    }
}
