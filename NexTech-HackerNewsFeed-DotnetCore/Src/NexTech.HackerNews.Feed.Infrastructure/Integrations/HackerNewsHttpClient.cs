using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using NexTech.HackerNews.Feed.Application.Interfaces;
using System.Net.Http.Json;

namespace NexTech.HackerNews.Feed.Infrastructure.Integrations
{
    /// <summary>
    /// Typed HttpClient for HackerNews API.
    /// Configured through IHttpClientFactory via DI in ServiceCollectionExtensions.
    /// This class performs GET calls, handles JSON deserialization, logs telemetry,
    /// and throws exceptions on non-success HTTP responses.
    /// </summary>
    public class HackerNewsHttpClient : IHackerNewsHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HackerNewsHttpClient> _logger;
        private readonly TelemetryClient? _telemetryClient;

        public HackerNewsHttpClient(
            HttpClient httpClient,
            ILogger<HackerNewsHttpClient> logger,
            TelemetryClient? telemetryClient = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Performs a GET request and deserializes JSON payload into type T.
        /// Automatically logs telemetry and propagates HttpRequestException on failure.
        /// </summary>
        public async Task<T?> GetFromJsonAsync<T>(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Calling HackerNews API: {Path}", path);

                // Perform the request
                using var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);

                // Throws if not 2xx
                response.EnsureSuccessStatusCode();

                // Deserialize JSON response
                var payload = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogDebug("Received response from HackerNews API: {Path} (HasPayload: {HasPayload})", path, payload != null);

                return payload;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error when calling HackerNews API: {Path}", path);

                // Minimal telemetry for failure
                _telemetryClient?.TrackEvent("HackerNewsHttpRequestFailed", new Dictionary<string, string?>
                {
                    ["Path"] = path,
                    ["Message"] = httpEx.Message,
                    ["StatusCode"] = httpEx.StatusCode?.ToString()
                }
                .Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value!));

                // Let middleware handle response translation
                throw;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout scenario
                _logger.LogWarning(ex, "Timeout while calling HackerNews API: {Path}", path);
                _telemetryClient?.TrackEvent("HackerNewsTimeout", new Dictionary<string, string>() { ["Path"] = path });
                throw new HttpRequestException("The HackerNews API request timed out.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling HackerNews API: {Path}", path);
                _telemetryClient?.TrackException(ex);
                throw;
            }
        }
    }
}