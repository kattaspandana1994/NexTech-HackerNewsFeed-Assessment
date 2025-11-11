namespace NexTech.HackerNews.Feed.Api.Extensions
{
    using NexTech.HackerNews.Feed.Application.Interfaces;
    using NexTech.HackerNews.Feed.Application.Services;
    using NexTech.HackerNews.Feed.Api.Mappings;
    using NexTech.HackerNews.Feed.Infrastructure.Integrations;

    /// <summary>
    /// Defines the <see cref="ServiceCollectionExtensions" />
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// The AddHackerNewsServices
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        /// <param name="configuration">The configuration<see cref="IConfiguration"/></param>
        /// <returns>The <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddHackerNewsServices(this IServiceCollection services, IConfiguration configuration)
        {                   
            //Cache Service
            services.AddScoped<ICache, NexTech.HackerNews.Feed.Infrastructure.Caching.RedisCache>();
            // Story Service
            services.AddScoped<INewsStoryService, NewsStoryService>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<StoryMapper>();
            });

            return services;
        }
    }
}
