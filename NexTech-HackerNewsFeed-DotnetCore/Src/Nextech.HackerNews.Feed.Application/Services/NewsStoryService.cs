namespace NexTech.HackerNews.Feed.Application.Services
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NexTech.HackerNews.Feed.Application.DTOs;
    using NexTech.HackerNews.Feed.Application.Interfaces;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service responsible for retrieving HackerNews stories and caching them (multi-page).
    /// </summary>
    public class NewsStoryService : INewsStoryService
    {
        private readonly IHackerNewsApiClient _apiClient;
        private readonly ICache _cache;
        private readonly ILogger<NewsStoryService> _logger;        
        private readonly string _cacheKeyBase;
        private readonly TimeSpan _cacheTtl;
        private readonly int _maxConcurrency;
        private readonly int _maxStoriesToWarm;   // how many stories the warmer/cache will aim to keep
        private readonly int _pageSizeConfig;     // page size used for creating per-page cache keys (default 10)

        /// <summary>
        /// Initializes a new instance of <see cref="NewsStoryService"/>.
        /// </summary>
        public NewsStoryService(
            IHackerNewsApiClient apiClient,
            ICache cache,
            IConfiguration configuration,
            ILogger<NewsStoryService> logger
           )
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            

            // configuration
            var cacheSection = configuration.GetSection("CacheSettings");
            _cacheKeyBase = cacheSection.GetValue<string>("TopStoriesCacheKey", "topstories");
            var ttlMin = cacheSection.GetValue<int>("TopStoriesCacheDurationMinutes", 5);
            _cacheTtl = TimeSpan.FromMinutes(ttlMin);

            var hnSection = configuration.GetSection("HackerNews");
            _maxConcurrency = hnSection.GetValue<int>("Concurrency:MaxConcurrentRequests", 10);
            _maxStoriesToWarm = hnSection.GetValue<int>("MaxStoriesToCache", 200); // how many to warm in full cache
            _pageSizeConfig = cacheSection.GetValue<int>("PageSize", 10);

            _logger.LogInformation(
                "NewsStoryService initialized. CacheKey='{CacheKey}', TTL={TTL}m, MaxConcurrency={Concurrency}, WarmSize={WarmSize}, PageSize={PageSize}",
                _cacheKeyBase, _cacheTtl.TotalMinutes, _maxConcurrency, _maxStoriesToWarm, _pageSizeConfig);
        }

        #region Public API

        /// <summary>
        /// Returns up to <paramref name="requiredCount"/> top valid stories (with URLs).
        /// The returned PagedResult.Items contains up to requiredCount items;
        /// PagedResult.Count contains total number of valid stories available (for pagination).
        /// </summary>
        public async Task<PagedResult<HackerNewsStory>> GetNewestNewStoriesAsync(int requiredCount, CancellationToken cancellationToken = default)
        {
            if (requiredCount <= 0) requiredCount = _pageSizeConfig;

            // Keys
            var allKey = $"{_cacheKeyBase}_all";
            var totalKey = $"{_cacheKeyBase}_totalcount";

            try
            {
                // Try to use full-list cache first (fast)
                var allStories = await _cache.GetAsync<List<HackerNewsStory>>(allKey, cancellationToken);
                var totalCount = await _cache.GetAsync<int?>(totalKey, cancellationToken) ?? 0;

                if (allStories != null && allStories.Count > 0)
                {
                    _logger.LogDebug("Serving {Required} stories from full cache (cached count: {CachedCount}).", requiredCount, allStories.Count);
                    return new PagedResult<HackerNewsStory>
                    {
                        Items = allStories.Take(requiredCount).ToList(),
                        Count = totalCount
                    };
                }

                // Full-list cache missing. Try warming up (use at least requiredCount or warm-size)
                var warmTarget = Math.Max(requiredCount, _maxStoriesToWarm);
                _logger.LogInformation("Cache miss for full list. Triggering refresh for top {WarmTarget} items.", warmTarget);

                // Refresh (this will populate per-page keys and full cache)
                await RefreshTopStoriesCacheAsync(warmTarget, cancellationToken).ConfigureAwait(false);

                // Re-read from cache
                allStories = await _cache.GetAsync<List<HackerNewsStory>>(allKey, cancellationToken);
                totalCount = await _cache.GetAsync<int?>(totalKey, cancellationToken) ?? (allStories?.Count ?? 0);

                if (allStories != null)
                {
                    return new PagedResult<HackerNewsStory>
                    {
                        Items = allStories.Take(requiredCount).ToList(),
                        Count = totalCount
                    };
                }

                // As a last resort, attempt to fetch minimally (requiredCount) directly - resilient fallback
                _logger.LogWarning("Full cache still empty after refresh. Falling back to fetching top {RequiredCount} stories directly.", requiredCount);
                var ids = await _apiClient.GetTopStoriesAsync(cancellationToken);
                if (ids == null || ids.Count == 0)
                {
                    return new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };
                }

                var fetchIds = ids.Take(requiredCount).ToList();
                var fetched = await FetchValidStoriesAsync(fetchIds, cancellationToken);
                return new PagedResult<HackerNewsStory>
                {
                    Items = fetched.Take(requiredCount).ToList(),
                    Count = fetched.Count
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetNewestNewStoriesAsync was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetNewestNewStoriesAsync");                
                // return empty to upstream rather than throw (controller/middleware will handle as needed)
                return new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };
            }
        }

        /// <summary>
        /// Refreshes and repopulates the cache. It will populate:
        /// - {cacheKey}_all (full list up to topCount)
        /// - {cacheKey}_totalcount (int)
        /// - {cacheKey}_page_{n} for each page according to configured page size
        /// This method swallows errors and logs them (designed for background execution).
        /// </summary>
        public async Task RefreshTopStoriesCacheAsync(int topCount, CancellationToken cancellationToken = default)
        {
            if (topCount <= 0) topCount = _maxStoriesToWarm;
            var allKey = $"{_cacheKeyBase}_all";
            var totalKey = $"{_cacheKeyBase}_totalcount";

            _logger.LogInformation("Refreshing top {TopCount} stories for cache (max concurrency: {Concurrency})", topCount, _maxConcurrency);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var ids = await _apiClient.GetTopStoriesAsync(cancellationToken);
                if (ids == null || ids.Count == 0)
                {
                    _logger.LogWarning("HackerNews returned zero IDs when attempting refresh.");
                    return;
                }

                // Limit IDs to requested topCount
                var idsToFetch = ids.Take(topCount).ToList();

                // Concurrent fetch (throttled)
                var validStories = await FetchValidStoriesAsync(idsToFetch, cancellationToken);

                // Order by time descending (newest first)
                var ordered = validStories.OrderByDescending(s => s.Time).ToList();

                // Cache full list and total count
                await _cache.SetAsync(allKey, ordered, _cacheTtl, cancellationToken);
                await _cache.SetAsync(totalKey, ordered.Count, _cacheTtl, cancellationToken);

                // Cache per-page slices
                var pageSize = Math.Max(1, _pageSizeConfig);
                var totalPages = (int)Math.Ceiling((double)ordered.Count / pageSize);

                for (int p = 1; p <= totalPages; p++)
                {
                    var pageKey = $"{_cacheKeyBase}_page_{p}";
                    var items = ordered.Skip((p - 1) * pageSize).Take(pageSize).ToList();
                    await _cache.SetAsync(pageKey, items, _cacheTtl, cancellationToken);
                }

                stopwatch.Stop();
                _logger.LogInformation("Refresh complete: {ValidCount} valid stories cached across {Pages} pages in {ElapsedMs}ms",
                    ordered.Count, totalPages, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("RefreshTopStoriesCacheAsync canceled via token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh top stories cache.");                
            }
        }

        /// <summary>
        /// Search stories (case-insensitive) among cached valid stories and return the requested page.
        /// This relies on the full cache (or will trigger a refresh if missing).
        /// </summary>
        public async Task<PagedResult<HackerNewsStory>> SearchStoriesAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };

            try
            {
                // Ensure full cache exists
                var allKey = $"{_cacheKeyBase}_all";
                var totalKey = $"{_cacheKeyBase}_totalcount";

                var allStories = await _cache.GetAsync<List<HackerNewsStory>>(allKey, cancellationToken);
                if (allStories == null)
                {
                    // warm at least enough to answer the query (use configured warm size)
                    await RefreshTopStoriesCacheAsync(Math.Max(_maxStoriesToWarm, page * pageSize), cancellationToken).ConfigureAwait(false);
                    allStories = await _cache.GetAsync<List<HackerNewsStory>>(allKey, cancellationToken);
                }

                if (allStories == null || allStories.Count == 0)
                    return new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };

                // Filter case-insensitive on Title (and ensure Url present)
                var filtered = allStories
                    .Where(s => !string.IsNullOrWhiteSpace(s.Url) &&
                                s.Title != null &&
                                s.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var totalMatches = filtered.Count;
                var skip = Math.Max(0, (page - 1)) * Math.Max(1, pageSize);
                var pageItems = filtered.Skip(skip).Take(pageSize).ToList();

                return new PagedResult<HackerNewsStory>
                {
                    Items = pageItems,
                    Count = totalMatches
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SearchStoriesAsync canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchStoriesAsync failed.");               
                return new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Fetches story details for the provided IDs concurrently (throttled) and returns only stories with non-empty Url.
        /// This method respects cancellation and logs per-id failures.
        /// </summary>
        private async Task<List<HackerNewsStory>> FetchValidStoriesAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var bag = new ConcurrentBag<HackerNewsStory>();
            var throttler = new SemaphoreSlim(_maxConcurrency);
            var tasks = ids.Select(async id =>
            {
                await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var story = await _apiClient.GetStoryByIdAsync(id, cancellationToken).ConfigureAwait(false);
                    if (story != null && !string.IsNullOrWhiteSpace(story.Url))
                    {
                        bag.Add(story);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // propagate cancellation up
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch story id {StoryId}", id);
                }
                finally
                {
                    throttler.Release();
                }
            }).ToList();

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("FetchValidStoriesAsync canceled while fetching details.");
                throw;
            }

            return bag.ToList();
        }

        #endregion
    }
}