using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexTech.HackerNews.Feed.Application.DTOs;
using NexTech.HackerNews.Feed.Application.Interfaces;
using NexTech.HackerNews.Feed.Infrastructure.Models;

namespace NexTech.HackerNews.Feed.Infrastructure.Integrations
{
    /// <summary>
    /// Concrete implementation of IHackerNewsApiClient that uses HackerNewsHttpClient.
    /// </summary>
    public class HackerNewsApiClient : IHackerNewsApiClient
    {
        private readonly IHackerNewsHttpClient _client;
        private readonly HackerNewsOptions _options;
        private readonly ILogger<HackerNewsApiClient> _logger;

        public HackerNewsApiClient(
            IHackerNewsHttpClient client,
            IOptions<HackerNewsOptions> options,
            ILogger<HackerNewsApiClient> logger)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Fetches top story IDs from Hacker News API.
        /// </summary>
        public async Task<List<int>> GetTopStoriesAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = Combine(_options.BaseUrl, _options.Endpoints.TopStories);

            _logger.LogInformation("Fetching top stories from {Endpoint}", endpoint);

            var ids = await _client.GetFromJsonAsync<List<int>>(endpoint, cancellationToken);

            _logger.LogInformation("Retrieved {Count} top story IDs", ids?.Count ?? 0);

            return ids ?? new List<int>();
        }

        /// <summary>
        /// Fetches a single story by ID.
        /// </summary>
        public async Task<HackerNewsStory?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var endpointTemplate = Combine(_options.BaseUrl, _options.Endpoints.StoryById);
            var endpoint = string.Format(endpointTemplate, id);

            _logger.LogDebug("Fetching story by ID {Id} from {Endpoint}", id, endpoint);

            return await _client.GetFromJsonAsync<HackerNewsStory>(endpoint, cancellationToken);
        }

        /// <summary>
        /// Safely combines base URL and relative paths.
        /// </summary>
        private static string Combine(string baseUrl, string relative)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return relative;
            return $"{baseUrl.TrimEnd('/')}/{relative.TrimStart('/')}";
        }
    }
}
