namespace NexTech.HackerNews.Feed.Application.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="IHackerNewsHttpClient" />
    /// </summary>
    public interface IHackerNewsHttpClient
    {
        /// <summary>
        /// The GetFromJsonAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{T?}"/></returns>
        Task<T?> GetFromJsonAsync<T>(string path, CancellationToken cancellationToken = default);
    }

}