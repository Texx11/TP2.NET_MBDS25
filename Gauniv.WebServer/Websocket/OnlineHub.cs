using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Gauniv.WebServer.Data;
using System.Collections.Concurrent;

namespace Gauniv.WebServer.Websocket
{
    public class OnlineStatus
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
    }

    public class OnlineHub : Hub
    {
        // Dictionnaire thread-safe pour suivre les connexions actives
        private static readonly ConcurrentDictionary<string, OnlineStatus> ConnectedUsers = new();

        private readonly UserManager<User> _userManager;

        public OnlineHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext?.User?.Identity is not null && httpContext.User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(httpContext.User);
                var userName = _userManager.GetUserName(httpContext.User);
                var status = new OnlineStatus
                {
                    UserId = userId,
                    UserName = userName,
                    ConnectedAt = DateTime.UtcNow
                };
                // Ajoute la connexion identifiée par Context.ConnectionId
                ConnectedUsers[Context.ConnectionId] = status;
                // Notifie tous les clients avec la liste mise à jour
                await Clients.All.SendAsync("UpdateUserStatus", ConnectedUsers.Values);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Retire la connexion du dictionnaire
            ConnectedUsers.TryRemove(Context.ConnectionId, out _);
            // Notifie tous les clients avec la liste mise à jour
            await Clients.All.SendAsync("UpdateUserStatus", ConnectedUsers.Values);
            await base.OnDisconnectedAsync(exception);
        }

        // Permet au client de récupérer la liste actuelle des utilisateurs en ligne
        public Task GetOnlineUsers()
        {
            return Clients.Caller.SendAsync("UpdateUserStatus", ConnectedUsers.Values);
        }
    }
}
