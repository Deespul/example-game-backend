using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class Match
    {
        public string MatchId { get; set; }
        public bool IsFinished { get; set; }
        public string Winner { get; set; }
        public string QueueId { get; set; }
        public List<Team> Teams { get; set; }
        public List<Team> TeamsAfterMmrUpdate { get; set; }
    }
}