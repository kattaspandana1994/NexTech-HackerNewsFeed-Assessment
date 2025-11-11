using NexTech.HackerNews.Feed.Application.DTOs;

namespace NexTech.HackerNews.Feed.Application.Interfaces
{
    /// <summary>
    /// Defines the <see cref="IHackerNewsApiClient" />
    /// </summary>
    public interface IHackerNewsApiClient
    {
        /// <summary>
        /// The GetTopStoriesAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{List{int}}"/></returns>
        Task<List<int>> GetTopStoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// The GetStoryByIdAsync
        /// </summary>
        /// <param name="id">The id<see cref="int"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{HackerNewsStory?}"/></returns>
        Task<HackerNewsStory?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
