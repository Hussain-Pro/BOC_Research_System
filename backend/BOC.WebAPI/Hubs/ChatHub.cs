using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Enums;

namespace BOC.WebAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IBOCDbContext _dbContext;

    public ChatHub(IBOCDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SendMessage(Guid channelId, Guid? receiverId, string content)
    {
        var senderIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (senderIdClaim == null || !Guid.TryParse(senderIdClaim.Value, out var senderId))
        {
            throw new HubException("Unauthorized.");
        }

        var senderRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var channel = await _dbContext.ChatChannels
            .FirstOrDefaultAsync(c => c.Id == channelId);
        if (channel == null)
        {
            throw new HubException("Channel not found.");
        }

        if (receiverId.HasValue)
        {
            var receiver = await _dbContext.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == receiverId.Value);

            if (receiver != null)
            {
                var receiverRole = receiver.Role.Name;

                // Section 6, Rule 8: Evaluator-to-Researcher messaging is hard-blocked
                var isSenderEvaluator = senderRole == "External Evaluators";
                var isReceiverResearcher = receiverRole == "Researchers";

                if (isSenderEvaluator && isReceiverResearcher)
                {
                    throw new HubException("Direct communication between Evaluators and Researchers is strictly forbidden.");
                }
            }
        }

        if (receiverId.HasValue)
        {
            await Clients.User(receiverId.Value.ToString()).SendAsync("ReceiveMessage", senderId, content);
        }
        else
        {
            await Clients.Group(channelId.ToString()).SendAsync("ReceiveMessage", senderId, content);
        }
    }

    public async Task JoinChannel(Guid channelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, channelId.ToString());
    }

    public async Task LeaveChannel(Guid channelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId.ToString());
    }
}
