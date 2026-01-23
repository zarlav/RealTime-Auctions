using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace RealTime_Auctions.Hubs
{
    public class AuctionHub : Hub
    {
        // Ovim metodom klijenti se mogu "priključiti" na određenu aukciju
        public async Task JoinAuction(string auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
        }

        public async Task LeaveAuction(string auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
        }
    }
}
