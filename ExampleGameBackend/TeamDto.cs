using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class TeamDto
    {
        public List<MatchPlayerDto> Players { get; set; }
        public int TeamId { get; set; }
    }
    
    public class MatchPlayerDto
    {
        public PlayerIdentificationDto PlayerId { get; set; }
        public MmrDto Mmr { get; set; }
        public string TeamId { get; set; }
    }
}