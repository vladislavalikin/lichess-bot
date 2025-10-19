namespace chessbot.LichessBot.Models.GameEventModels
{

    public class GameStateEvent
    {
        public string type { get; set; }
        public string moves { get; set; }
        public int wtime { get; set; }
        public int btime { get; set; }
        public int winc { get; set; }
        public int binc { get; set; }
        public bool wdraw { get; set; }
        public bool bdraw { get; set; }
        public bool wtakeback { get; set; }
        public bool btakeback { get; set; }
        public string status { get; set; }
    }
}