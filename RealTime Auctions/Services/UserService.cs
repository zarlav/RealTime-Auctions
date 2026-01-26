using RealTime_Auctions.Models;
using StackExchange.Redis;

namespace RealTime_Auctions.Services;

public class UserService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public UserService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task CreateUserAsync(User user)
    {
        var key = $"user:{user.Id}";
        var entries = new HashEntry[]
        {
            new HashEntry("id", user.Id),
            new HashEntry("username", user.Username),
            new HashEntry("email", user.Email),
            new HashEntry("password", user.Password)
        };

        await _db.HashSetAsync(key, entries);
        await _db.SetAddAsync("users:all", user.Id);
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var userIds = await _db.SetMembersAsync("users:all");

        foreach (var id in userIds)
        {
            var userEntries = await _db.HashGetAllAsync($"user:{id}");
            var storedUsername = userEntries.FirstOrDefault(e => e.Name == "username").Value;
            var storedPassword = userEntries.FirstOrDefault(e => e.Name == "password").Value;

            if (storedUsername == username && storedPassword == password)
            {
                return new User
                {
                    Id = id.ToString(),
                    Username = storedUsername.ToString(),
                    Email = userEntries.FirstOrDefault(e => e.Name == "email").Value.ToString(),
                    Password = storedPassword.ToString()
                };
            }
        }
        return null; 
    }
}