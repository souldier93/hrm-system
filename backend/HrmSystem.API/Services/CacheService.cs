using StackExchange.Redis;
using System.Text.Json;

namespace HrmSystem.API.Services;

public class CacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(5));
            _logger.LogInformation("Cache SET: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError("Cache SET error: {Message}", ex.Message);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.LogInformation("Cache MISS: {Key}", key);
                return default;
            }
            _logger.LogInformation("Cache HIT: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError("Cache GET error: {Message}", ex.Message);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
            _logger.LogInformation("Cache REMOVE: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError("Cache REMOVE error: {Message}", ex.Message);
        }
    }
}