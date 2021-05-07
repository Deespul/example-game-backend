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

        public GameController(GameHub gameHub)
        {
            _gameHub = gameHub;
        }

        [HttpPost("matches-report")]
        public async Task<ActionResult> ReportMatches([FromBody] List<MatchFound> matchesFound)
        {
            await _gameHub.ReportMatchFoundToPlayers(matchesFound);
            return Ok();
        }
    }

    public class MatchFound
    {
        public string QueueId { get; set; }
        public string MatchId { get; set; }
        public string Id { get; set; }
        public List<Team> Teams { get; set; }
        public DateTimeOffset? ReportedBackAt { get; set; }
    }
    public class Team
    {
        public string TeamId { get; set; }
        public List<string> PlayerIds { get; set; }
    }
}