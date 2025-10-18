namespace chessbot.Models.StreamEventModels
{
    public class LCGameStartedEvent
    {
        public string type { get; set; }
        public Game game { get; set; }
    }
}
