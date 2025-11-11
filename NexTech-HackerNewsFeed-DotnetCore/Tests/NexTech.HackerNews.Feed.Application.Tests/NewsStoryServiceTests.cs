using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NexTech.HackerNews.Feed.Application.DTOs;
using NexTech.HackerNews.Feed.Application.Interfaces;
using NexTech.HackerNews.Feed.Application.Services;

namespace NexTech.HackerNews.Feed.Tests.Application.Services
{
    public class NewsStoryServiceTests
    {
        private readonly Mock<IHackerNewsApiClient> _mockApiClient;
        private readonly Mock<ICache> _mockCache;
        private readonly Mock<ILogger<NewsStoryService>> _mockLogger;
        private readonly IConfiguration _configuration;
        private readonly NewsStoryService _sut;

        public NewsStoryServiceTests()
        {
            _mockApiClient = new Mock<IHackerNewsApiClient>();
            _mockCache = new Mock<ICache>();
            _mockLogger = new Mock<ILogger<NewsStoryService>>();

            var settings = new Dictionary<string, string?>
            {
                { "CacheSettings:TopStoriesCacheKey", "topstories_test" },
                { "CacheSettings:TopStoriesCacheDurationMinutes", "5" },
                { "CacheSettings:PageSize", "10" },
                { "HackerNews:Concurrency:MaxConcurrentRequests", "3" },
                { "HackerNews:MaxStoriesToCache", "50" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _sut = new NewsStoryService(
                _mockApiClient.Object,
                _mockCache.Object,
                _configuration,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetNewestNewStoriesAsync_ReturnsFromCache_WhenAvailable()
        {
            // Arrange
            var allStories = new List<HackerNewsStory>
            {
                new() { Id = 1, Title = "Cached 1", Url = "https://news/1" },
                new() { Id = 2, Title = "Cached 2", Url = "https://news/2" }
            };

            _mockCache.Setup(c => c.GetAsync<List<HackerNewsStory>>("topstories_test_all", It.IsAny<CancellationToken>()))
                .ReturnsAsync(allStories);
            _mockCache.Setup(c => c.GetAsync<int?>("topstories_test_totalcount", It.IsAny<CancellationToken>()))
                .ReturnsAsync(allStories.Count);

            // Act
            var result = await _sut.GetNewestNewStoriesAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.Count);
            _mockApiClient.Verify(a => a.GetTopStoriesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetNewestNewStoriesAsync_RefreshesCache_WhenCacheMiss()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<List<HackerNewsStory>>("topstories_test_all", It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<HackerNewsStory>?)null);
            _mockCache.Setup(c => c.GetAsync<int?>("topstories_test_totalcount", It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            _mockApiClient.Setup(a => a.GetTopStoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 1, 2, 3 });

            _mockApiClient.Setup(a => a.GetStoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => new HackerNewsStory
                {
                    Id = id,
                    Title = $"Story {id}",
                    Url = $"https://news/{id}",
                    Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetNewestNewStoriesAsync(3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Items.Count > 0);
            _mockApiClient.Verify(a => a.GetTopStoriesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RefreshTopStoriesCacheAsync_CachesOnlyValidStories()
        {
            // Arrange
            _mockApiClient.Setup(a => a.GetTopStoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 1, 2, 3 });

            _mockApiClient.Setup(a => a.GetStoryByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => id == 2
                    ? new HackerNewsStory { Id = 2, Title = "Valid", Url = "https://valid/2" }
                    : new HackerNewsStory { Id = id, Title = $"Story {id}", Url = "" });

            // Act
            await _sut.RefreshTopStoriesCacheAsync(3);

            // Assert
            _mockCache.Verify(c => c.SetAsync(
                "topstories_test_all",
                It.Is<List<HackerNewsStory>>(l => l.All(s => !string.IsNullOrWhiteSpace(s.Url))),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchStoriesAsync_ReturnsEmpty_WhenQueryIsEmpty()
        {
            // Act
            var result = await _sut.SearchStoriesAsync("", 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public async Task SearchStoriesAsync_PerformsCaseInsensitiveMatch()
        {
            // Arrange
            var cachedStories = new List<HackerNewsStory>
            {
                new() { Id = 1, Title = "AI Revolution", Url = "https://1" },
                new() { Id = 2, Title = "C# Performance", Url = "https://2" },
                new() { Id = 3, Title = "ai future", Url = "https://3" }
            };

            _mockCache.Setup(c => c.GetAsync<List<HackerNewsStory>>("topstories_test_all", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedStories);

            // Act
            var result = await _sut.SearchStoriesAsync("ai", 1, 10);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.True(result.Items.All(s => s.Title.Contains("ai", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task SearchStoriesAsync_SkipsStoriesWithoutUrl()
        {
            // Arrange
            var cachedStories = new List<HackerNewsStory>
            {
                new() { Id = 1, Title = "AI News", Url = "https://link" },
                new() { Id = 2, Title = "AI News 2", Url = "" }
            };

            _mockCache.Setup(c => c.GetAsync<List<HackerNewsStory>>("topstories_test_all", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedStories);

            // Act
            var result = await _sut.SearchStoriesAsync("AI", 1, 10);

            // Assert
            Assert.Single(result.Items);
            Assert.True(result.Items.All(s => !string.IsNullOrWhiteSpace(s.Url)));
        }

        [Fact]
        public async Task SearchStoriesAsync_ReturnsPagedResults()
        {
            // Arrange
            var stories = Enumerable.Range(1, 20)
                .Select(i => new HackerNewsStory { Id = i, Title = $"Story {i}", Url = $"https://valid/{i}" })
                .ToList();

            _mockCache.Setup(c => c.GetAsync<List<HackerNewsStory>>("topstories_test_all", It.IsAny<CancellationToken>()))
                .ReturnsAsync(stories);

            // Act
            var result = await _sut.SearchStoriesAsync("Story", 2, 5);

            // Assert
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(20, result.Count);
        }

        [Fact]
        public async Task RefreshTopStoriesCacheAsync_HandlesApiFailureGracefully()
        {
            // Arrange
            _mockApiClient.Setup(a => a.GetTopStoriesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act & Assert
            await _sut.RefreshTopStoriesCacheAsync(5); // should not throw
        }

        [Fact]
        public async Task GetNewestNewStoriesAsync_HandlesNoIdsGracefully()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<List<HackerNewsStory>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<HackerNewsStory>?)null);
            _mockApiClient.Setup(a => a.GetTopStoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int>());

            // Act
            var result = await _sut.GetNewestNewStoriesAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }
    }
}
