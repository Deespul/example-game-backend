using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ExampleGameBackend
{
    [Route("api")]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<GameHub> _gameHub;
        private readonly MatchCache _matchCache;
        private readonly ConnectionCache _connectionCache;

        public GameController(IHubContext<GameHub> gameHub, MatchCache matchCache, ConnectionCache connectionCache)
        {
            _gameHub = gameHub;
            _matchCache = matchCache;
            _connectionCache = connectionCache;
        }

        [HttpPost("matches-report")]
        public async Task<ActionResult> ReportMatches([FromBody] List<MatchDto> matchesFound)
        {
            foreach (var matchFound in matchesFound)
            {
                var playerIds = matchFound.Teams.SelectMany(t => t.Players).Select(p => p.PlayerId);
                var connectionIds = playerIds.SelectMany(id => _connectionCache.Where(p => p.Value.PlayerId == id).Select(r => r.Key)).ToList();
                await _gameHub.Clients.Clients(connectionIds).SendAsync("MatchFound", matchFound);
            }

            _matchCache.Add(matchesFound);
            return Ok();
        }
        
        [HttpGet("matches-report")]
        public ActionResult<List<MatchDto>> GetMatches()
        {
            return Ok(_matchCache.Matches?.OrderByDescending(m => m.MatchId));
        }
    }

    public class MatchCache
    {
        public List<MatchDto> Matches = new();

        public void Add(List<MatchDto> matchesFound)
        {
            Matches.AddRange(matchesFound);
        }
    }
}