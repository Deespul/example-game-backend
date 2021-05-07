using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ExampleGameBackend
{
    [Route("api")]
    public class GameController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly GameHub _gameHub;

        public GameController(HttpClient httpClient, GameHub gameHub)
        {
            _httpClient = httpClient;
            _gameHub = gameHub;
        }

        [HttpPost("matches-report")]
        public async Task<ActionResult> ReportMatches([FromBody] List<MatchFound> matchesFound)
        {
            _gameHub.ReportMatchFoundToPlayers(matchesFound);
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