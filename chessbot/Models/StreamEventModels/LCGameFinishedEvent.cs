namespace chessbot.Models.StreamEventModels
{

    public class LCGameFinishedEvent
    {
        public string type { get; set; }
        public Game game { get; set; }
    }
}
