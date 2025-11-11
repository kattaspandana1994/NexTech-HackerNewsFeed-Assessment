namespace NexTech.HackerNews.Feed.Application.DTOs
{
    /// <summary>
    /// Generic paged result wrapper used across layers (Application → API).
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// Total count of available items (for pagination).
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The list of items in this page.
        /// </summary>
        public List<T> Items { get; set; } = new();
       
    }
}
