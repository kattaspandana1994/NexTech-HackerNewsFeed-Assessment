namespace NexTech.HackerNews.Feed.Application.Interfaces
{
    using NexTech.HackerNews.Feed.Application.DTOs;

    /// <summary>
    /// Defines the <see cref="INewsStoryService" />
    /// </summary>
    public interface INewsStoryService
    {
        /// <summary>
        /// The GetNewestNewStoriesAsync
        /// </summary>
        /// <param name="count">The count<see cref="int"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{StoryList}"/></returns>
        Task<PagedResult<HackerNewsStory>> GetNewestNewStoriesAsync(int count, CancellationToken cancellationToken);

        /// <summary>
        /// The SearchStoriesAsync
        /// </summary>
        /// <param name="query">The query<see cref="string"/></param>
        /// <param name="pageNumber">The pageNumber<see cref="int"/></param>
        /// <param name="pageSize">The pageSize<see cref="int"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{StoryList}"/></returns>
        Task<PagedResult<HackerNewsStory>> SearchStoriesAsync(
        string query,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);


        /// <summary>
       ///explicit cache refresh method
        /// </summary>
        /// <param name="topCount">The topCount<see cref="int"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task RefreshTopStoriesCacheAsync(int topCount, CancellationToken cancellationToken = default);
    }
}
