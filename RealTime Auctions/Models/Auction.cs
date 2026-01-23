namespace RealTime_Auctions.Models
{
    public class Auction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ProductName { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public decimal StartPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public string LeaderUserId { get; set; } = string.Empty;
        public DateTime StartAt { get; set; } = DateTime.UtcNow;
        public DateTime EndAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
        public string Status { get; set; } = "active";
    }
}
