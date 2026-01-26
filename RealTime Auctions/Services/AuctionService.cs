using RealTime_Auctions.Models;
using RealTime_Auctions.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;

namespace RealTime_Auctions.Services;

public class AuctionService
{
    private readonly IDatabase _db;
    private readonly IHubContext<AuctionHub> _hubContext;

    public AuctionService(IConnectionMultiplexer redis, IHubContext<AuctionHub> hubContext)
    {
        _db = redis.GetDatabase();
        _hubContext = hubContext;
    }

    public async Task CreateAuctionAsync(Auction auction)
    {
        var key = $"auction:{auction.Id}";
        await _db.StringSetAsync(key, JsonSerializer.Serialize(auction));
        await _db.SetAddAsync("auctions:active", auction.Id);
        await _hubContext.Clients.All.SendAsync("NewAuctionCreated", auction);

        _ = Task.Run(async () =>
        {
            var delay = auction.EndAt - DateTime.UtcNow;
            if (delay > TimeSpan.Zero) await Task.Delay(delay);
            await EndAuctionAsync(auction.Id);
        });
    }

    public async Task PlaceBidAsync(string auctionId, string userId, decimal amount)
    {
        var key = $"auction:{auctionId}";
        var data = await _db.StringGetAsync(key);
        if (data.IsNullOrEmpty) return;

        var auction = JsonSerializer.Deserialize<Auction>(data!);

        if (amount < auction.CurrentPrice + 10) return;

        if (auction.Status != "active" || DateTime.UtcNow > auction.EndAt) return;

        auction.CurrentPrice = amount;
        auction.LeaderUserId = userId;

        await _db.StringSetAsync(key, JsonSerializer.Serialize(auction));
        await _hubContext.Clients.All.SendAsync("NewBid", auction);
    }

    public async Task DeleteAuctionAsync(string auctionId)
    {
        var key = $"auction:{auctionId}";
        await _db.KeyDeleteAsync(key);
        await _db.SetRemoveAsync("auctions:active", auctionId);

        await _hubContext.Clients.All.SendAsync("AuctionDeleted", auctionId);
    }

    public async Task EndAuctionAsync(string auctionId)
    {
        var key = $"auction:{auctionId}";
        var data = await _db.StringGetAsync(key);
        if (data.IsNullOrEmpty) return;

        var auction = JsonSerializer.Deserialize<Auction>(data!);
        auction.Status = "ended";
        await _db.StringSetAsync(key, JsonSerializer.Serialize(auction));
        await _db.SetRemoveAsync("auctions:active", auctionId);

        await _hubContext.Clients.All.SendAsync("AuctionEnded", auction);
    }

    public async Task<List<Auction>> GetActiveAuctionsAsync()
    {
        var ids = await _db.SetMembersAsync("auctions:active");
        var list = new List<Auction>();
        foreach (var id in ids)
        {
            var data = await _db.StringGetAsync($"auction:{id}");
            if (!data.IsNullOrEmpty) list.Add(JsonSerializer.Deserialize<Auction>(data!)!);
        }
        return list;
    }
}