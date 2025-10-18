namespace chessbot.Models.StreamEventModels
{
    public class LCChallange
    {
        public string type { get; set; }
        public Challenge challenge { get; set; }
        public Compat compat { get; set; }
    }

    public class Challenge
    {
        public string id { get; set; }
        public string url { get; set; }
        public string status { get; set; }
        public Challenger challenger { get; set; }
        public Destuser destUser { get; set; }
        public Variant variant { get; set; }
        public bool rated { get; set; }
        public string speed { get; set; }
        public Timecontrol timeControl { get; set; }
        public string color { get; set; }
        public string finalColor { get; set; }
        public Perf perf { get; set; }
    }

    public class Challenger
    {
        public string name { get; set; }
        public string flair { get; set; }
        public string id { get; set; }
        public int rating { get; set; }
    }

    public class Destuser
    {
        public string name { get; set; }
        public string id { get; set; }
        public int rating { get; set; }
        public bool online { get; set; }
    }

    public class Variant
    {
    }

    public class Timecontrol
    {
    }

    public class Perf
    {
    }

    public class Compat
    {
        public bool bot { get; set; }
        public bool board { get; set; }
    }
}
