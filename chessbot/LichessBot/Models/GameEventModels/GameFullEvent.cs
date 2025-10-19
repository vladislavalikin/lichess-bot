namespace chessbot.LichessBot.Models.GameEventModels
{

    public class GameFullEvent
    {
        public string id { get; set; }
        public Variant variant { get; set; }
        public string speed { get; set; }
        public Perf perf { get; set; }
        public bool rated { get; set; }
        public long createdAt { get; set; }
        public White white { get; set; }
        public Black black { get; set; }
        public string initialFen { get; set; }
        public Clock clock { get; set; }
        public string type { get; set; }
        public State state { get; set; }
    }

    public class Variant
    {
        public string key { get; set; }
        public string name { get; set; }
        public string _short { get; set; }
    }

    public class Perf
    {
        public string name { get; set; }
    }

    public class White
    {
        public string id { get; set; }
        public string name { get; set; }
        public object title { get; set; }
        public int rating { get; set; }
    }

    public class Black
    {
        public string id { get; set; }
        public string name { get; set; }
        public object title { get; set; }
        public int rating { get; set; }
    }

    public class Clock
    {
        public int initial { get; set; }
        public int increment { get; set; }
    }

    public class State
    {
        public string type { get; set; }
        public string moves { get; set; }
        public int wtime { get; set; }
        public int btime { get; set; }
        public int winc { get; set; }
        public int binc { get; set; }
        public string status { get; set; }
    }

}
