namespace RealTime_Auctions.Models
{
    public class Bid
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string AuctionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
