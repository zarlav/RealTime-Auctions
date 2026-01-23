using RealTime_Auctions.Config;
using RealTime_Auctions.Models;
using StackExchange.Redis;

namespace RealTime_Auctions.Services
{
    public class UserService
    {
        private readonly IDatabase _db;

        public UserService(RedisService redis)
        {
            _db = redis.Db;
        }

        public async Task CreateUserAsync(User user)
        {
            await _db.HashSetAsync(RealTime_Auctions.Config.RedisKeys.User(user.Id), new HashEntry[]
            {
            new("id", user.Id),
            new("username", user.Username),
            new("email", user.Email)
            });

            await _db.SetAddAsync(RealTime_Auctions.Config.RedisKeys.UsersAll, user.Id);
        }

        public async Task<User?> GetUserAsync(string userId)
        {
            var entries = await _db.HashGetAllAsync(RealTime_Auctions.Config.RedisKeys.User(userId));
            if (entries.Length == 0) return null;

            return new User
            {
                Id = entries.FirstOrDefault(e => e.Name == "id").Value,
                Username = entries.FirstOrDefault(e => e.Name == "username").Value,
                Email = entries.FirstOrDefault(e => e.Name == "email").Value
            };
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var ids = await _db.SetMembersAsync(RealTime_Auctions.Config.RedisKeys.UsersAll);
            var users = new List<User>();

            foreach (var id in ids)
            {
                var user = await GetUserAsync(id);
                if (user != null) users.Add(user);
            }

            return users;
        }
    }

}