using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class MatchDto
    {
        public string MatchId { get; set; }
        public bool IsFinished { get; set; }
        public int? Winner { get; set; }
        public string QueueId { get; set; }
        public List<MatchTeamDto> Teams { get; set; }
        public List<MatchTeamDto> TeamsAfterMmrUpdate { get; set; }
        public List<long> WaitTimes { get; set; }
        public double AverageWaitTime { get; set; }
    }
    
    public class MatchTeamDto
    {
        public List<MatchPlayerDto> Players { get; set; }
        public int TeamId { get; set; }
    }
    
    public class MmrDto
    {
        public double Rating { get; set; }
        public double RatingDeviation { get; set; }
    }
    
    public class TeamDto
    {
        public List<MatchPlayerDto> Players { get; set; }
        public int TeamId { get; set; }
    }
    
    public class MatchPlayerDto
    {
        public PlayerIdentificationDto PlayerId { get; set; }
        public string Faction { get; set; }
        public MmrDto Mmr { get; set; }
        public string TeamId { get; set; }
    }
}