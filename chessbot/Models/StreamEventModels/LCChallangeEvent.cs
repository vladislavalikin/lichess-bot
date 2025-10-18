namespace chessbot.Models.StreamEventModels
{
    public class LCChallangeEvent
    {
        public string type { get; set; }
        public Challenge challenge { get; set; }
        public Compat compat { get; set; }
    }
}
