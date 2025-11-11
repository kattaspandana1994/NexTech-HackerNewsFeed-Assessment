namespace NexTech.HackerNews.Feed.Infrastructure.Tests.Integrations
{
    using FluentAssertions;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NexTech.HackerNews.Feed.Infrastructure.Integrations;
    using NexTech.HackerNews.Feed.Infrastructure.Tests.TestUtilities;
    using System.Net;
    using System.Net.Http;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="HackerNewsHttpClientTests" />
    /// </summary>
    public class HackerNewsHttpClientTests
    {
        /// <summary>
        /// Defines the _mockLogger
        /// </summary>
        private readonly Mock<ILogger<HackerNewsHttpClient>> _mockLogger = new();

        /// <summary>
        /// Defines the _fakeTelemetry
        /// </summary>
        private readonly TelemetryClient _fakeTelemetry = new(new TelemetryConfiguration());

        /// <summary>
        /// The GetFromJsonAsync_ReturnsDeserializedObject_OnSuccess
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        [Fact]
        public async Task GetFromJsonAsync_ReturnsDeserializedObject_OnSuccess()
        {
            var mockResponse = new { id = 1, title = "Sample Story" };
            var json = System.Text.Json.JsonSerializer.Serialize(mockResponse);

            var handler = new MockHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.api/") };
            var sut = new HackerNewsHttpClient(httpClient, _mockLogger.Object, _fakeTelemetry);

            var story = await sut.GetFromJsonAsync<TestStory>("stories");            
            story?.Id.Should().Be(1);
            story?.Title.Should().Be("Sample Story");
        }


        /// <summary>
        /// The GetFromJsonAsync_ThrowsHttpRequestException_On404
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        [Fact]        
        public async Task GetFromJsonAsync_ThrowsHttpRequestException_On404()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.api/") };
            var sut = new HackerNewsHttpClient(httpClient, _mockLogger.Object, _fakeTelemetry);

            // Act + Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetFromJsonAsync<object>("invalid"));
        }

    }

    public class TestStory
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
