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
    private const decimal DefaultBidIncrement = 10m;

    public AuctionService(IConnectionMultiplexer redis, IHubContext<AuctionHub> hubContext)
    {
        _db = redis.GetDatabase();
        _hubContext = hubContext;
    }

    private static Auction? DeserializeAuction(RedisValue data)
    {
        if (data.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<Auction>(data);
    }

    public async Task CreateAuctionAsync(Auction auction)
    {
        var key = $"auction:{auction.Id}";

        await _db.StringSetAsync(key, JsonSerializer.Serialize(auction));

        await _db.SortedSetAddAsync("auctions:active", auction.Id, auction.EndAt.Ticks);

        await _hubContext.Clients.All.SendAsync("NewAuctionCreated", auction);

        _ = Task.Run(async () =>
        {
            var delay = auction.EndAt - DateTime.UtcNow;
            if (delay > TimeSpan.Zero) await Task.Delay(delay);
            await EndAuctionAsync(auction.Id);
        });
    }

    public async Task<bool> PlaceBidAsync(string auctionId, string userId, decimal amount)
    {
        var key = $"auction:{auctionId}";
        var data = await _db.StringGetAsync(key);
        var auction = DeserializeAuction(data);
        if (auction == null) return false;

        if (auction.Status != AuctionStatusEnum.Active || DateTime.UtcNow > auction.EndAt)
            return false;

        if (amount < auction.CurrentPrice + DefaultBidIncrement)
            return false;

        var oldVersion = auction.Version;
        auction.CurrentPrice = amount;
        auction.LeaderUserId = userId;
        auction.Version++;

        await _db.StringSetAsync(key, JsonSerializer.Serialize(auction));

        await _hubContext.Clients.All.SendAsync("NewBid", auction);

        return true;
    }

    public async Task DeleteAuctionAsync(string auctionId)
    {
        var key = $"auction:{auctionId}";
        await _db.KeyDeleteAsync(key);
        await _db.SortedSetRemoveAsync("auctions:active", auctionId);
        await _hubContext.Clients.All.SendAsync("AuctionDeleted", auctionId);
    }

    public async Task EndAuctionAsync(string auctionId)
    {
        var key = $"auction:{auctionId}";
        var data = await _db.StringGetAsync(key);
        var auction = DeserializeAuction(data);
        if (auction == null) return;

        auction.Status = AuctionStatusEnum.Finished;
        auction.Version++;

        await _db.StringSetAsync(key, JsonSerializer.Serialize(auction));
        await _db.SortedSetRemoveAsync("auctions:active", auctionId);

        await _hubContext.Clients.All.SendAsync("AuctionEnded", auction);
    }

    public async Task<List<Auction>> GetActiveAuctionsAsync()
    {

        var ids = await _db.SortedSetRangeByScoreAsync("auctions:active");

        var auctions = new List<Auction>();
        foreach (var id in ids)
        {
            var data = await _db.StringGetAsync($"auction:{id}");
            var auction = DeserializeAuction(data);
            if (auction != null)
                auctions.Add(auction);
        }

        return auctions;
    }
}
