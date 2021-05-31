using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class MatchFound
    {
        public string QueueId { get; set; }
        public List<Team> Teams { get; set; }
        public string MatchId { get; set; }
    }
}