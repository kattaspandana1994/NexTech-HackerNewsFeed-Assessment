namespace NexTech.HackerNews.Feed.Infrastructure.Models
{
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Configuration for HackerNews API integration
    /// </summary>
    public class HackerNewsOptions
    {
        /// <summary>
        /// Gets the BaseUrl
        /// </summary>
        public string BaseUrl { get; init; } = string.Empty;

        /// <summary>
        /// Gets the Endpoints
        /// </summary>
        public EndpointsConfig Endpoints { get; init; } = new();

        /// <summary>
        /// Gets the Timeout
        /// </summary>
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Defines the <see cref="EndpointsConfig" />
        /// </summary>
        public class EndpointsConfig
        {
            /// <summary>
            /// Gets the TopStories
            /// </summary>
            public string TopStories { get; init; } = string.Empty;

            /// <summary>
            /// Gets the StoryById
            /// </summary>
            public string StoryById { get; init; } = string.Empty;
        }
    }

    /// <summary>
    /// Ensures required configuration is present and valid at startup
    /// </summary>
    public class HackerNewsOptionsValidator : IValidateOptions<HackerNewsOptions>
    {
        /// <summary>
        /// The Validate
        /// </summary>
        /// <param name="name">The name<see cref="string?"/></param>
        /// <param name="options">The options<see cref="HackerNewsOptions"/></param>
        /// <returns>The <see cref="ValidateOptionsResult"/></returns>
        public ValidateOptionsResult Validate(string? name, HackerNewsOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                return ValidateOptionsResult.Fail("HackerNews:BaseUrl must be provided.");

            if (options.Endpoints is null)
                return ValidateOptionsResult.Fail("HackerNews:Endpoints section is required.");

            if (string.IsNullOrWhiteSpace(options.Endpoints.TopStories))
                return ValidateOptionsResult.Fail("HackerNews:Endpoints:TopStories is missing.");

            if (string.IsNullOrWhiteSpace(options.Endpoints.StoryById))
                return ValidateOptionsResult.Fail("HackerNews:Endpoints:StoryById is missing.");

            return ValidateOptionsResult.Success;
        }
    }
}
