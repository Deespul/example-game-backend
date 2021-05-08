using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ExampleGameBackend
{
    [Route("api")]
    public class GameController : ControllerBase
    {
        private readonly GameHub _gameHub;
        private readonly MatchCache _matchCache;

        public GameController(GameHub gameHub, MatchCache matchCache)
        {
            _gameHub = gameHub;
            _matchCache = matchCache;
        }

        [HttpPost("matches-report")]
        public async Task<ActionResult> ReportMatches([FromBody] List<MatchFound> matchesFound)
        {
            _matchCache.Add(matchesFound);
            await _gameHub.ReportMatchFoundToPlayers(matchesFound);
            return Ok();
        }
        
        [HttpGet("matches-report")]
        public ActionResult GetMatches()
        {
            return Ok(_matchCache.Matches);
        }
    }

    public class MatchCache
    {
        public List<MatchFound> Matches = new();

        public void Add(List<MatchFound> matchesFound)
        {
            Matches.AddRange(matchesFound);
        }
    }

    public class MatchFound
    {
        public string QueueId { get; set; }
        public string MatchId { get; set; }
        public List<Team> Teams { get; set; }
        public DateTimeOffset? ReportedBackAt { get; set; }
    }
    public class Team
    {
        public string TeamId { get; set; }
        public List<string> PlayerIds { get; set; }
    }
}