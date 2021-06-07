using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class MatchFound
    {
        public string QueueId { get; set; }
        public string MatchId { get; set; }
        public List<TeamDto> Teams { get; set; }
        public List<TeamDto> TeamsAfterMmrUpdate { get; set; }
        public int Winner { get; set; }
    }
}