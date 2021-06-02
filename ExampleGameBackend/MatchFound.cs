using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class MatchFound
    {
        public string QueueId { get; set; }
        public string MatchId { get; set; }
        public List<Team> Teams { get; set; }
        public List<Team> TeamsAfterMmrUpdate { get; set; }
        public string Winner { get; set; }
    }
}