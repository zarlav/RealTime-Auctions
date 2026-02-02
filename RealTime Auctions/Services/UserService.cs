using RealTime_Auctions.Models;
using StackExchange.Redis;

namespace RealTime_Auctions.Services;

public class UserService
{
    private readonly IDatabase _db;
    private const string UsernamesSetKey = "usernames:all";

    public UserService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return await _db.SetContainsAsync(UsernamesSetKey, username.ToLower().Trim());
    }

    public async Task CreateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }

        var userKey = $"user:{user.Id}";
        var entries = new HashEntry[]
        {
            new HashEntry("id", user.Id),
            new HashEntry("username", user.Username),
            new HashEntry("email", user.Email),
            new HashEntry("password", user.Password)
        };

        await _db.HashSetAsync(userKey, entries);
        await _db.SetAddAsync("users:all", user.Id);
        await _db.SetAddAsync(UsernamesSetKey, user.Username.ToLower().Trim());
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var userIds = await _db.SetMembersAsync("users:all");
        foreach (var id in userIds)
        {
            var entries = await _db.HashGetAllAsync($"user:{id}");
            if (entries.Length == 0) continue;

            var storedUser = entries.FirstOrDefault(e => e.Name == "username").Value;
            var storedPass = entries.FirstOrDefault(e => e.Name == "password").Value;

            if (storedUser.ToString().Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) &&
                storedPass == password)
            {
                return new User
                {
                    Id = id.ToString(),
                    Username = storedUser.ToString(),
                    Email = entries.FirstOrDefault(e => e.Name == "email").Value.ToString(),
                    Password = storedPass.ToString()
                };
            }
        }
        return null;
    }
}