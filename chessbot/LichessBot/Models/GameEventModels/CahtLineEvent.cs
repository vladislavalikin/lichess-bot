namespace chessbot.LichessBot.Models.GameEventModels
{

    public class ChatLineEvent
    {
        public string type { get; set; }
        public string username { get; set; }
        public string text { get; set; }
        public string room { get; set; }
    }
}
