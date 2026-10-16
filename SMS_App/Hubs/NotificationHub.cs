using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SMS_App.Hubs
{
    /// <summary>
    /// Pushes in-app notifications to the browser in real time. Each connection joins a group
    /// named after the signed-in user's id (Context.UserIdentifier, which Identity sets to the
    /// AspNetUsers.Id), so the server can target "this user" regardless of how many tabs/devices
    /// they have open. Clients only receive; they never invoke anything, so there are no hub methods.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public const string ReceiveNotification = "receiveNotification";

        public static string GroupFor(string userId) => $"user:{userId}";

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId));
            }

            await base.OnConnectedAsync();
        }
    }
}
