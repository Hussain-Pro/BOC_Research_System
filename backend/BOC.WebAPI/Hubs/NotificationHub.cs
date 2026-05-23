using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BOC.WebAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}
