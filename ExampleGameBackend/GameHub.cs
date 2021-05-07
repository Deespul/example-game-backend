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
        private readonly Dictionary<string, PlayerDto> _connections = new();

        public GameHub(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
            var player = _connections[Context.ConnectionId];
            _connections.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("PlayerLeft",  player);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task LoginAs(PlayerDto player)
        {
            _connections.Add(Context.ConnectionId, player);
            await Clients.All.SendAsync("PlayerEntered",  player);
        }

        public async Task RegisterGame(EnqueueCommand command)
        {
            var result = await _httpClient.PutAsJsonAsync($"/queues/{command.QueueId}", command);

            if (result.IsSuccessStatusCode)
            {
                await Clients.Caller.SendAsync("Enqueued");
            }
            else
            {
                await Clients.Caller.SendAsync("EnqueueFailed");
            }
        }

        public async Task ReportGame(MatchResult match)
        {
            var result = await _httpClient.PutAsJsonAsync($"/matches/{match.MatchId}", match);

            if (result.IsSuccessStatusCode)
            {
                await Clients.Caller.SendAsync("MatchReported");
            }
            else
            {
                await Clients.Caller.SendAsync("MatchReportFailed");
            }
        }
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