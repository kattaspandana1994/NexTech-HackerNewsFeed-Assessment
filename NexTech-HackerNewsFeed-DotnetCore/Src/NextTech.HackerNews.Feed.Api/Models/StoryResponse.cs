namespace NexTech.HackerNews.Feed.Api.Models
{
    /// <summary>
    /// Defines the <see cref="StoryResponse" />
    /// </summary>
    public class StoryResponse
    {
        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
