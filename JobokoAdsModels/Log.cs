using System.Collections.Generic;

namespace JobokoAdsModels
{
    public class Log
    {
        public Dictionary<string, Dictionary<string, object>> ext { get; set; }
        public long d { get; set; }

        public string ai { get; set; }

        public string n { get; set; }
        public Dictionary<string, Dictionary<string, object>> prox { get; set; }
    }
}