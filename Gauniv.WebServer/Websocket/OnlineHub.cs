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
        private readonly ApplicationDbContext _context;

        public OnlineHub(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

        /// <summary>
        /// Renvoie la liste de tous les utilisateurs (sauf les admins).
        /// </summary>
        public async Task GetAllUsersExceptAdmins()
        {
            var allUsers = _context.Users.ToList();
            var nonAdminUsers = new List<object>();

            foreach (var user in allUsers)
            {
                // On exclut les admins
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    nonAdminUsers.Add(new
                    {
                        // Exposez seulement ce dont vous avez besoin côté client
                        Id = user.Id,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    });
                }
            }

            // Envoie la liste de tous les utilisateurs (hors admin) au client qui a fait l'appel
            await Clients.Caller.SendAsync("AllUsers", nonAdminUsers);
        }

        /// <summary>
        /// Renvoie la liste des utilisateurs actuellement connectés.
        /// </summary>
        public Task GetOnlineUsers()
        {
            return Clients.Caller.SendAsync("UpdateUserStatus", ConnectedUsers.Values);
        }
    }
}
