using System.Collections.Generic;

namespace ExampleGameBackend
{
    public class Team
    {
        public string TeamId { get; set; }
        public List<PlayerIdentificationDto> PlayerIds { get; set; }
        public Mmr Mmr { get; set; }
    }
}