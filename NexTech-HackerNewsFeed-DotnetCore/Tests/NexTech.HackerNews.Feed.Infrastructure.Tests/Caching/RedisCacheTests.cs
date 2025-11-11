using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NexTech.HackerNews.Feed.Infrastructure.Caching;
using FluentAssertions;
using Xunit;

namespace NexTech.HackerNews.Feed.Infrastructure.Tests.Caching
{
    public class RedisCacheTests
    {
        private readonly Mock<IDistributedCache> _mockDistributedCache;
        private readonly Mock<ILogger<RedisCache>> _mockLogger;
        private readonly RedisCache _sut;

        public RedisCacheTests()
        {
            _mockDistributedCache = new Mock<IDistributedCache>();
            _mockLogger = new Mock<ILogger<RedisCache>>();
            _sut = new RedisCache(_mockDistributedCache.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsDefault_WhenCacheIsEmpty()
        {
            _mockDistributedCache
                .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null!);

            var result = await _sut.GetAsync<string>("missing-key");

            result.Should().BeNull();
        }

        [Fact]
        public async Task SetAsync_StoresCompressedSerializedValue()
        {
            var testObj = new { Id = 1, Name = "CacheTest" };
            await _sut.SetAsync("test-key", testObj, TimeSpan.FromMinutes(5));

            _mockDistributedCache.Verify(
                c => c.SetAsync(
                    "test-key",
                    It.IsAny<byte[]>(),
                    It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_CreatesValue_WhenCacheMiss()
        {
            _mockDistributedCache
                .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null!);

            var result = await _sut.GetOrCreateAsync("key", async () => "new-value", TimeSpan.FromMinutes(1));

            result.Should().Be("new-value");
            _mockDistributedCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}