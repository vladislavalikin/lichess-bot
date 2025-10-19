namespace chessbot.LichessBot.Models.GameEventModels
{

    public class OpponentGoneEvent
    {
        public string type { get; set; }
        public bool gone { get; set; }
        public int claimWinInSeconds { get; set; }
    }
}
