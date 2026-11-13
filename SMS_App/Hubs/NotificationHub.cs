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

        // Group names are built only on the server, from the resolved school and the authenticated
        // user id; clients cannot join groups (the hub has no methods). Two schools' databases can
        // contain the same user id, so the school is part of the name.
        public static string GroupFor(SMS.BLL.MultiTenancy.ITenantContext tenant, string userId) =>
            SMS.BLL.MultiTenancy.TenantContextExtensions.Scoped(tenant, $"user:{userId}");

        private readonly SMS.BLL.MultiTenancy.ITenantContext _tenant;

        public NotificationHub(SMS.BLL.MultiTenancy.ITenantContext tenant)
        {
            _tenant = tenant;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(_tenant, userId));
            }

            await base.OnConnectedAsync();
        }
    }
}
