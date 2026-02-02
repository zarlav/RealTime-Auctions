using RealTime_Auctions.Models;
using StackExchange.Redis;

namespace RealTime_Auctions.Services;

public class UserService
{
    private readonly IDatabase _db;
    private const string UsernamesSetKey = "usernames:all";
    private const string UsersAllKey = "users:all";

    public UserService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        string normalized = username.ToLower().Trim();
        return await _db.SetContainsAsync(UsernamesSetKey, normalized);
    }

    public async Task CreateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }

        var userKey = $"user:{user.Id}";
        string normalizedUsername = user.Username.ToLower().Trim();

        var entries = new HashEntry[]
        {
            new HashEntry("id", user.Id),
            new HashEntry("username", user.Username.Trim()),
            new HashEntry("email", user.Email),
            new HashEntry("password", user.Password)
        };

        await _db.HashSetAsync(userKey, entries);
        await _db.SetAddAsync(UsersAllKey, user.Id);
        await _db.SetAddAsync(UsernamesSetKey, normalizedUsername);
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var userIds = await _db.SetMembersAsync(UsersAllKey);
        foreach (var id in userIds)
        {
            var entries = await _db.HashGetAllAsync($"user:{id}");
            if (entries.Length == 0) continue;

            var storedUser = entries.FirstOrDefault(e => e.Name == "username").Value.ToString();
            var storedPass = entries.FirstOrDefault(e => e.Name == "password").Value.ToString();

            if (storedUser.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) &&
                storedPass == password)
            {
                return new User
                {
                    Id = id.ToString(),
                    Username = storedUser,
                    Email = entries.FirstOrDefault(e => e.Name == "email").Value.ToString(),
                    Password = storedPass
                };
            }
        }
        return null;
    }
}