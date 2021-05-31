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

        public GameController(IHubContext<GameHub> gameHub, MatchCache matchCache)
        {
            _gameHub = gameHub;
            _matchCache = matchCache;
        }

        [HttpPost("matches-report")]
        public async Task<ActionResult> ReportMatches([FromBody] List<MatchFound> matchesFound)
        {
            foreach (var matchFound in matchesFound)
            {
                var selectMany = matchFound.Teams.SelectMany(t => t.PlayerIds);
                await _gameHub.Clients.Clients(selectMany).SendAsync("MatchFound", matchFound);
            }

            _matchCache.Add(matchesFound);
            return Ok();
        }
        
        [HttpGet("matches-report")]
        public ActionResult<List<MatchFound>> GetMatches()
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
}