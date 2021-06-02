using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ExampleGameBackend
{
    public class GameHub : Hub
    {
        private readonly HttpClient _httpClient;
        private readonly MatchCache _matchCache;
        private readonly ConnectionCache _connectionCache;
        private readonly Dictionary<string, UnfinishedMatchResult> _timeReported;

        public GameHub(MatchCache matchCache, ConnectionCache connectionCache, HttpClient httpClient, Dictionary<string, UnfinishedMatchResult> timeReported)
        {
            _httpClient = httpClient;
            _timeReported = timeReported;
            _matchCache = matchCache;
            _connectionCache = connectionCache;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = _connectionCache[Context.ConnectionId];
            _connectionCache.Remove(Context.ConnectionId);
            await _httpClient.DeleteAsync($"enqueued-teams/{player.PlayerId}");
            await Clients.All.SendAsync("PlayerLeft",  player);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task LoginAs(PlayerDto player)
        {
            // if (_connectionCache.ContainsKey(Context.ConnectionId))
            // {
            //     _connectionCache.Remove(Context.ConnectionId);
            //     await Clients.All.SendAsync("PlayerLeft",  player);
            // }
            
            _connectionCache.Add(Context.ConnectionId, player);
            await Clients.Others.SendAsync("PlayerEntered",  player);
            await Clients.Caller.SendAsync("LoggedIn", new { onlinePlayers = _connectionCache.Values });
        }

        public async Task Enqueue(EnqueueCommand command)
        {
            var result = await _httpClient.PostAsJsonAsync("enqueued-teams?api_key=secret", command);
            
            if (result.IsSuccessStatusCode)
            {
                await Clients.Caller.SendAsync("Enqueued");
            }
            else
            {
                await Clients.Caller.SendAsync("EnqueueFailed");
            }
        }

        public async Task ReportGame(double time, string matchId)
        {
            var player = _connectionCache[Context.ConnectionId];
            var matchOfPlayer = _matchCache.Matches.Single(m => m.MatchId == matchId);

            if (_timeReported.ContainsKey(matchOfPlayer.MatchId))
            {
                var unfinishedMatchResult = _timeReported[matchOfPlayer.MatchId];
                unfinishedMatchResult.Add(player.PlayerId, time);
                var finishedMatch = unfinishedMatchResult.FinishedMatch(matchOfPlayer.MatchId);

                var result = await _httpClient.PutAsJsonAsync($"/matches/{matchOfPlayer.MatchId}?api_key=secret", finishedMatch);
                var playerIds = finishedMatch.Teams.Select(t => t.TeamId);
                var connectiosnFromPlayers = _connectionCache.Where(c => playerIds.Contains(c.Value.PlayerId)).Select(c => c.Key);
                if (result.IsSuccessStatusCode)
                {
                    var matchResult = await result.Content.ReadFromJsonAsync<Match>();
                    await Clients.Clients(connectiosnFromPlayers).SendAsync("MatchFinished", matchResult);
                }
                else
                {
                    await Clients.Caller.SendAsync("MatchReportFailed");
                }
            }
            else
            {
                var unfinishedMatchResult = new UnfinishedMatchResult();
                unfinishedMatchResult.Add(player.PlayerId, time);
                _timeReported.Add(matchOfPlayer.MatchId, unfinishedMatchResult);
                await Clients.Caller.SendAsync("PartialResultReported");
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
        public string PlayerId { get; set; }
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