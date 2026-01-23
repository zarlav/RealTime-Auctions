using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealTime_Auctions.Config;
using RealTime_Auctions.Models;
using RealTime_Auctions.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace RealTime_Auctions.Services
{
    public class AuctionService
    {
        private readonly IDatabase _db;
        private readonly IHubContext<AuctionHub> _hub;

        public AuctionService(RedisService redis, IHubContext<AuctionHub> hub)
        {
            _db = redis.Db;
            _hub = hub;
        }

        // Kreiranje aukcije
        public async Task CreateAuctionAsync(Auction auction)
        {
            await _db.HashSetAsync(RedisKeys.Auction(auction.Id), new HashEntry[]
            {
                new("id", auction.Id),
                new("productName", auction.ProductName),
                new("ownerId", auction.OwnerId),
                new("startPrice", auction.StartPrice.ToString()),
                new("currentPrice", auction.CurrentPrice.ToString()),
                new("leaderUserId", auction.LeaderUserId),
                new("startAt", auction.StartAt.ToString("o")),
                new("endAt", auction.EndAt.ToString("o")),
                new("status", auction.Status)
            });

            await _db.SetAddAsync(RedisKeys.AuctionsActive, auction.Id);
        }

        // Dohvati aukciju po ID-u
        public async Task<Auction?> GetAuctionAsync(string auctionId)
        {
            var entries = await _db.HashGetAllAsync(RedisKeys.Auction(auctionId));
            if (entries.Length == 0) return null;

            return new Auction
            {
                Id = entries.FirstOrDefault(e => e.Name == "id").Value,
                ProductName = entries.FirstOrDefault(e => e.Name == "productName").Value,
                OwnerId = entries.FirstOrDefault(e => e.Name == "ownerId").Value,
                StartPrice = decimal.Parse(entries.FirstOrDefault(e => e.Name == "startPrice").Value),
                CurrentPrice = decimal.Parse(entries.FirstOrDefault(e => e.Name == "currentPrice").Value),
                LeaderUserId = entries.FirstOrDefault(e => e.Name == "leaderUserId").Value,
                StartAt = DateTime.Parse(entries.FirstOrDefault(e => e.Name == "startAt").Value),
                EndAt = DateTime.Parse(entries.FirstOrDefault(e => e.Name == "endAt").Value),
                Status = entries.FirstOrDefault(e => e.Name == "status").Value
            };
        }

        // Baci bid
        public async Task PlaceBidAsync(string auctionId, string userId, decimal amount)
        {
            var auction = await GetAuctionAsync(auctionId);
            if (auction == null) throw new Exception("Auction not found");
            if (auction.Status != "active") throw new Exception("Auction not active");
            if (amount <= auction.CurrentPrice) throw new Exception("Bid too low");

            // update bid
            auction.CurrentPrice = amount;
            auction.LeaderUserId = userId;

            await _db.HashSetAsync(RedisKeys.Auction(auctionId), new HashEntry[]
            {
                new("currentPrice", auction.CurrentPrice.ToString()),
                new("leaderUserId", auction.LeaderUserId)
            });

            // dodaj u sorted set
            await _db.SortedSetAddAsync(RedisKeys.AuctionBids(auctionId), userId, (double)amount);

            // dodaj u set učesnika
            await _db.SetAddAsync(RedisKeys.AuctionParticipants(auctionId), userId);

            // 🔥 SignalR live update
            await _hub.Clients.Group(auctionId).SendAsync("NewBid", new
            {
                AuctionId = auctionId,
                UserId = userId,
                Amount = amount,
                CurrentPrice = auction.CurrentPrice,
                LeaderUserId = auction.LeaderUserId
            });

            // opcionalno: provjeri završetak aukcije
            await CheckAndFinishAuctionAsync(auctionId);
        }

        // Dohvati sve bidove
        public async Task<IEnumerable<Bid>> GetBidsAsync(string auctionId)
        {
            var entries = await _db.SortedSetRangeByScoreWithScoresAsync(RedisKeys.AuctionBids(auctionId), order: Order.Descending);
            return entries.Select(e => new Bid
            {
                AuctionId = auctionId,
                UserId = e.Element,
                Amount = (decimal)e.Score
            });
        }

        // Dohvati top N bidova
        public async Task<IEnumerable<Bid>> GetTopBidsAsync(string auctionId, int topN = 5)
        {
            var entries = await _db.SortedSetRangeByScoreWithScoresAsync(
                RedisKeys.AuctionBids(auctionId),
                order: Order.Descending,
                take: topN
            );

            return entries.Select(e => new Bid
            {
                AuctionId = auctionId,
                UserId = e.Element,
                Amount = (decimal)e.Score
            });
        }

        // Dohvati učesnike
        public async Task<IEnumerable<string>> GetParticipantsAsync(string auctionId)
        {
            var participants = await _db.SetMembersAsync(RedisKeys.AuctionParticipants(auctionId));
            return participants.Select(x => (string)x);
        }

        // Dohvati sve aktivne aukcije
        public async Task<IEnumerable<Auction>> GetActiveAuctionsAsync()
        {
            var ids = await _db.SetMembersAsync(RedisKeys.AuctionsActive);
            var auctions = new List<Auction>();

            foreach (var id in ids)
            {
                var auction = await GetAuctionAsync(id);
                if (auction != null) auctions.Add(auction);
            }

            return auctions;
        }

        // Automatsko završavanje aukcije
        public async Task CheckAndFinishAuctionAsync(string auctionId)
        {
            var auction = await GetAuctionAsync(auctionId);
            if (auction == null) return;

            if (DateTime.UtcNow >= auction.EndAt && auction.Status == "active")
            {
                auction.Status = "ended";
                await _db.HashSetAsync(RedisKeys.Auction(auctionId), "status", "ended");

                // premesti iz active u ended set
                await _db.SetRemoveAsync(RedisKeys.AuctionsActive, auctionId);
                await _db.SetAddAsync(RedisKeys.AuctionsEnded, auctionId);

                // notify clients
                await _hub.Clients.Group(auctionId).SendAsync("AuctionEnded", new
                {
                    AuctionId = auctionId,
                    WinnerUserId = auction.LeaderUserId,
                    WinningBid = auction.CurrentPrice
                });
            }
        }
    }
}
