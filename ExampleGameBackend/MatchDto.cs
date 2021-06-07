using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class MatchDto
    {
        
        public string MatchId { get; set; }
        public bool IsFinished { get; set; }
        public string Winner { get; set; }
        public string QueueId { get; set; }
        public List<TeamDto> Teams { get; set; }
        public List<TeamDto> TeamsAfterMmrUpdate { get; set; }
    }
}