using StackExchange.Redis;

namespace RealTime_Auctions.Services
{
    public class RedisService
    {
        public IDatabase Db { get; }

        public RedisService(IConnectionMultiplexer mux)
        {
            Db = mux.GetDatabase();
        }
    }
}
