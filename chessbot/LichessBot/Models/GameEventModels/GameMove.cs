

namespace chessbot.LichessBot.Models.GameEventModels
{

    public class GameMove
    {
        public Class1[] Property1 { get; set; }
    }

    public class Class1
    {
        public string id { get; set; }
        public Variant variant { get; set; }
        public string speed { get; set; }
        public string perf { get; set; }
        public bool rated { get; set; }
        public string initialFen { get; set; }
        public string fen { get; set; }
        public string player { get; set; }
        public int turns { get; set; }
        public int startedAtTurn { get; set; }
        public string source { get; set; }
        public Status status { get; set; }
        public long createdAt { get; set; }
        public string lastMove { get; set; }
        public Players players { get; set; }
        public int wc { get; set; }
        public int bc { get; set; }
        public string lm { get; set; }
    }

    public class Status
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Players
    {
        public White white { get; set; }
        public Black black { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public string title { get; set; }
        public string id { get; set; }
    }

    public class User1
    {
        public string name { get; set; }
        public string id { get; set; }
    }

}
