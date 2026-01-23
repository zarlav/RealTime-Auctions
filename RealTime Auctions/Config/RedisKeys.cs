namespace RealTime_Auctions.Config
{
    public static class RedisKeys
    {
        public static string Auction(string id) => $"auction:{id}";
        public static string AuctionBids(string id) => $"auction:{id}:bids";
        public static string AuctionParticipants(string id) => $"auction:{id}:participants";
        public static string User(string id) => $"user:{id}";
        public static string UsersAll => "users:all";
        public static string AuctionsActive => "auctions:active";
        public static string AuctionsEnded => "auctions:ended";
    }
}
