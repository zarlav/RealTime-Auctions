using RealTime_Auctions.Models;
using StackExchange.Redis;
using BCrypt.Net;

namespace RealTime_Auctions.Services;

public class UserService
{
    private readonly IDatabase _db;
    private const string UsersByIdSetKey = "users:all";
    private const string UsernamesSetKey = "usernames:all";
    private const string UserKeyPrefix = "user:id:";
    private const string UserByUsernameKeyPrefix = "user:username:";

    public UserService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }
    private static string GetHashValue(HashEntry[] entries, string field) =>
        entries.FirstOrDefault(e => e.Name == field).Value.ToString() ?? string.Empty;
    public async Task<bool> UserExistsAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        string normalized = username.ToLower().Trim();
        return await _db.SetContainsAsync(UsernamesSetKey, normalized);
    }
    public async Task CreateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Id))
            user.Id = Guid.NewGuid().ToString();

        string normalizedUsername = user.Username.ToLower().Trim();
        var userByIdKey = UserKeyPrefix + user.Id;
        var userByUsernameKey = UserByUsernameKeyPrefix + normalizedUsername;
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new ArgumentException("PasswordHash ne moze biti prazan!");
        }

        var entries = new HashEntry[]
        {
            new HashEntry("id", user.Id),
            new HashEntry("username", user.Username.Trim()),
            new HashEntry("email", user.Email),
            new HashEntry("passwordHash", user.PasswordHash) 
        };
        await _db.HashSetAsync(userByIdKey, entries);
        await _db.HashSetAsync(userByUsernameKey, entries);
        await _db.SetAddAsync(UsersByIdSetKey, user.Id);
        await _db.SetAddAsync(UsernamesSetKey, normalizedUsername);
    }
    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        string normalizedUsername = username.ToLower().Trim();
        string userByUsernameKey = UserByUsernameKeyPrefix + normalizedUsername;

        if (!await _db.KeyExistsAsync(userByUsernameKey))
            return null;

        var entries = await _db.HashGetAllAsync(userByUsernameKey);
        if (entries.Length == 0)
            return null;

        var storedHash = GetHashValue(entries, "passwordHash");
        if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
            return null;

        return new User
        {
            Id = GetHashValue(entries, "id"),
            Username = GetHashValue(entries, "username"),
            Email = GetHashValue(entries, "email"),
            PasswordHash = storedHash
        };
    }
    public async Task<User?> GetUserByIdAsync(string id)
    {
        var key = UserKeyPrefix + id;
        if (!await _db.KeyExistsAsync(key)) return null;

        var entries = await _db.HashGetAllAsync(key);
        if (entries.Length == 0) return null;

        return new User
        {
            Id = GetHashValue(entries, "id"),
            Username = GetHashValue(entries, "username"),
            Email = GetHashValue(entries, "email"),
            PasswordHash = GetHashValue(entries, "passwordHash")
        };
    }
}
