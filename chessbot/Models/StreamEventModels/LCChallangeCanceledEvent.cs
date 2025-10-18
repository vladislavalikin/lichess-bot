
namespace chessbot.Models.StreamEventModels
{

    public class LCChallangeCanceledEvent
    {
        public string type { get; set; }
        public Challenge challenge { get; set; }
    }
}
