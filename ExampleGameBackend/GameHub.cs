using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ExampleGameBackend
{
    public class GameHub : Hub
    {
        private readonly HttpClient _httpClient;
        private readonly MatchCache _matchCache;
        private readonly ConnectionCache _connectionCache;
        private readonly Dictionary<string, UnfinishedMatchResult> _timeReported = new();

        public GameHub(MatchCache matchCache, ConnectionCache connectionCache)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://matchbox.test.w3champions.com/");

            _matchCache = matchCache;
            _connectionCache = connectionCache;
        }

        public async Task ReportMatchFoundToPlayers(List<MatchFound> matchesFound)
        {
            foreach (var matchFound in matchesFound)
            {
                var selectMany = matchFound.Teams.SelectMany(t => t.PlayerIds);
                await Clients.Clients(selectMany).SendAsync("MatchFound", matchFound);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = _connectionCache[Context.ConnectionId];
            _connectionCache.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("PlayerLeft",  player);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task LoginAs(PlayerDto player)
        {
            _connectionCache.Add(Context.ConnectionId, player);
            await Clients.All.SendAsync("PlayerEntered",  player);
        }

        public async Task Enqueue(EnqueueCommand command)
        {
            var result = await _httpClient.PutAsJsonAsync($"queues/{command.QueueId}?api_key=secret", command);

            if (result.IsSuccessStatusCode)
            {
                await Clients.Caller.SendAsync("Enqueued");
            }
            else
            {
                await Clients.Caller.SendAsync("EnqueueFailed");
            }
        }

        public async Task ReportGame(double time)
        {
            var player = _connectionCache[Context.ConnectionId];
            var matchOfPlayer = _matchCache.Matches.Single(m => m.Teams.SelectMany(t => t.PlayerIds).Contains(player.Id));

            if (_timeReported.ContainsKey(matchOfPlayer.MatchId))
            {
                var unfinishedMatchResult = _timeReported[matchOfPlayer.MatchId];
                unfinishedMatchResult.Add(player.Id, time);
                var finishedMatch = unfinishedMatchResult.FinishedMatch(matchOfPlayer.MatchId);

                var result = await _httpClient.PutAsJsonAsync($"/matches/{matchOfPlayer.MatchId}?api_key=secret", finishedMatch);

                if (result.IsSuccessStatusCode)
                {
                    await Clients.Caller.SendAsync("MatchReported");
                }
                else
                {
                    await Clients.Caller.SendAsync("MatchReportFailed");
                }
            }
            else
            {
                var unfinishedMatchResult = new UnfinishedMatchResult();
                unfinishedMatchResult.Add(player.Id, time);
                _timeReported.Add(matchOfPlayer.MatchId, unfinishedMatchResult);
            }
        }
    }

    public class ConnectionCache : Dictionary<string, PlayerDto>
    {
    }

    public class UnfinishedMatchResult
    {
        private readonly Dictionary<string, double> _timeReported = new();
        public void Add(string playerId, double time)
        {
            _timeReported.Add(playerId, time);
        }

        public bool Finished => _timeReported.Count == 2;
        public MatchResult FinishedMatch(string matchId) => new ()
        {
            MatchId = matchId,
            Teams = new List<TeamReport>
            {
                new()
                {
                    TeamId = _timeReported.First().Key,
                    WonMatch = _timeReported.First().Value < _timeReported.Skip(1).First().Value
                },
                new()
                {
                    TeamId = _timeReported.Skip(1).First().Key,
                    WonMatch = _timeReported.Skip(1).First().Value < _timeReported.First().Value
                }
            }
        };

    }

    public class PlayerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class EnqueueCommand
    {
        public string QueueId { get; set; }
        public List<PlayerIdentificationDto> PlayerIds { get; set; }
    }

    public class PlayerIdentificationDto
    {
        public string PlayerId { get; set; }
        public string Faction { get; set; }
    }

    public class MatchResult
    {
        public string MatchId { get; set; }
        public List<TeamReport> Teams { get; set; }
    }

    public class TeamReport
    {
        public string TeamId { get; set; }
        public bool WonMatch { get; set; }
    }
}