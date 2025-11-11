namespace NexTech.HackerNews.Feed.Application.DTOs   
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Defines the <see cref="HackerNewsStory" />
    /// </summary>
    public class HackerNewsStory
    {
        /// <summary>
        /// Gets or sets the By
        /// </summary>
        [JsonPropertyName("by")]
        public string By { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Descendants
        /// </summary>
        [JsonPropertyName("descendants")]
        public int Descendants { get; set; }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Kids
        /// </summary>
        [JsonPropertyName("kids")]
        public List<int> Kids { get; set; } = [];

        /// <summary>
        /// Gets or sets the Score
        /// </summary>
        [JsonPropertyName("score")]
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the Time
        /// </summary>
        [JsonPropertyName("time")]
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
