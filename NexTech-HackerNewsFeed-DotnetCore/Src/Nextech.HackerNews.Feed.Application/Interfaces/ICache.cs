namespace NexTech.HackerNews.Feed.Application.Interfaces
{
    /// <summary>
    /// Defines the <see cref="ICache" />
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{T?}"/></returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// The SetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="T"/></param>
        /// <param name="ttl">The ttl<see cref="TimeSpan"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
    }
}
