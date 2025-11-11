using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NexTech.HackerNews.Feed.Application.DTOs;
using NexTech.HackerNews.Feed.Application.Interfaces;
using NexTech.HackerNews.Feed.Infrastructure.Integrations;
using NexTech.HackerNews.Feed.Infrastructure.Models;

namespace NexTech.HackerNews.Feed.Infrastructure.Tests.Integrations
{
    public class HackerNewsApiClientTests
    {
        private readonly Mock<IHackerNewsHttpClient> _mockHttpClient;
        private readonly Mock<ILogger<HackerNewsApiClient>> _mockLogger;
        private readonly IOptions<HackerNewsOptions> _options;
        private readonly HackerNewsApiClient _sut;

        public HackerNewsApiClientTests()
        {
            _mockHttpClient = new Mock<IHackerNewsHttpClient>();
            _mockLogger = new Mock<ILogger<HackerNewsApiClient>>();
            _options = Options.Create(new HackerNewsOptions
            {
                BaseUrl = "https://api.test/",
                Endpoints = new HackerNewsOptions.EndpointsConfig
                {
                    TopStories = "topstories.json",
                    StoryById = "item/{0}.json"
                }
            });

            _sut = new HackerNewsApiClient(_mockHttpClient.Object, _options, _mockLogger.Object);
        }

        [Fact]
        public async Task GetTopStoriesAsync_CallsCorrectEndpoint()
        {
            var expected = new List<int> { 1, 2, 3 };
            _mockHttpClient
                .Setup(c => c.GetFromJsonAsync<List<int>>("https://api.test/topstories.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.GetTopStoriesAsync();

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetStoryByIdAsync_CallsCorrectUrl()
        {
            var story = new HackerNewsStory { Id = 42, Title = "Test Story" };
            _mockHttpClient
                .Setup(c => c.GetFromJsonAsync<HackerNewsStory>("https://api.test/item/42.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync(story);

            var result = await _sut.GetStoryByIdAsync(42);

            result.Should().NotBeNull();
            result!.Id.Should().Be(42);
            result.Title.Should().Be("Test Story");
        }
    }
}
