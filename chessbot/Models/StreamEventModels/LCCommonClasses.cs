namespace chessbot.Models.StreamEventModels
{
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
        public string title { get; set; }
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
        public string key { get; set; }
        public string name { get; set; }
        public string _short { get; set; }
    }

    public class Timecontrol
    {
        public string type { get; set; }
    }

    public class Perf
    {
        public string icon { get; set; }
        public string name { get; set; }
    }

    public class Compat
    {
        public bool bot { get; set; }
        public bool board { get; set; }
    }
}
