using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NexTech.HackerNews.Feed.Application.Interfaces;
using StackExchange.Redis;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace NexTech.HackerNews.Feed.Infrastructure.Caching
{
    /// <summary>
    /// Distributed Redis cache wrapper providing serialization, compression, and helper methods.
    /// </summary>
    public class RedisCache(IDistributedCache cache, ILogger<RedisCache>? logger = null) : ICache
    {
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<RedisCache>? _logger = logger;
 
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Retrieve a value from cache.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedData = await _cache.GetAsync(key, cancellationToken);
                if (cachedData == null)
                {
                    _logger?.LogDebug("Cache MISS for key {Key}", key);
                    return default;
                }

                _logger?.LogDebug("Cache HIT for key {Key} (size: {Size} bytes)", key, cachedData.Length);

                // Decompress and deserialize
                var json = Decompress(cachedData);
                return JsonSerializer.Deserialize<T>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize cache entry for key {Key}. Removing entry.", key);
                await _cache.RemoveAsync(key, cancellationToken);
                return default;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected cache error for key {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Store a value in cache with TTL (compress before saving).
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(value, SerializerOptions);
                var compressed = Compress(json);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };

                await _cache.SetAsync(key, compressed, options, cancellationToken);               
               
                _logger?.LogDebug("Cache SET for key {Key} (TTL: {TTL} sec, compressed size: {Size} bytes)", key, ttl.TotalSeconds, compressed.Length);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to set cache for key {Key}", key);
            }
        }

        /// <summary>
        /// Get a value from cache or create and cache it atomically.
        /// Prevents redundant recomputation (GetOrCreate pattern).
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan ttl,
            CancellationToken cancellationToken = default)
        {
            var existing = await GetAsync<T>(key, cancellationToken);
            if (existing != null)
            {
                _logger?.LogDebug("Cache HIT in GetOrCreate for key {Key}", key);
                return existing;
            }

            _logger?.LogInformation("Cache MISS in GetOrCreate for key {Key} - invoking factory", key);
            var value = await factory();
            await SetAsync(key, value, ttl, cancellationToken);
            return value;
        }

        #region Compression Helpers

        /// <summary>
        /// Compresses a UTF8 JSON string to a byte array using GZip.
        /// </summary>
        private static byte[] Compress(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Fastest))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Decompresses a GZip byte array back to a JSON string.
        /// </summary>
        private static string Decompress(byte[] compressed)
        {
            using var input = new MemoryStream(compressed);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        #endregion
    }
}