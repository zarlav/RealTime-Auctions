namespace RealTime_Auctions.Models
{
    public class Auction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string ProductName { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;

        public decimal StartPrice { get; set; }
        public decimal CurrentPrice { get; set; }

        public string? LeaderUserId { get; set; }

        public DateTime StartAt { get; set; } = DateTime.UtcNow;
        public DateTime EndAt { get; set; }

        public AuctionStatusEnum Status { get; set; } = AuctionStatusEnum.Active;

        public long Version { get; set; } = 0;
    }

}
