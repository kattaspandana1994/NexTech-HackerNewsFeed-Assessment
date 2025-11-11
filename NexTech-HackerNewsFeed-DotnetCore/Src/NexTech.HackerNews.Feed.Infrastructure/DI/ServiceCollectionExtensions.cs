namespace NexTech.HackerNews.Feed.Infrastructure.DI
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using NexTech.HackerNews.Feed.Application.Interfaces;
    using NexTech.HackerNews.Feed.Infrastructure.Integrations;
    using NexTech.HackerNews.Feed.Infrastructure.Models;
    using Polly;
    using Polly.Extensions.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="ServiceCollectionExtensions" />
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// The AddInfrastructure to servicecollection
        /// </summary>
        /// <param name="services">The services<see cref="IServiceCollection"/></param>
        /// <param name="configuration">The configuration<see cref="IConfiguration"/></param>
        /// <returns>The <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddInfrastructureAsync(this IServiceCollection services, IConfiguration configuration)
        {
            // 🔧 Bind and validate HackerNews config
            services.AddOptions<HackerNewsOptions>()
                .Bind(configuration.GetSection("HackerNews"))
                .ValidateOnStart()
                .Services.AddSingleton<IValidateOptions<HackerNewsOptions>, HackerNewsOptionsValidator>();

            // 💾 Redis Cache            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
                options.InstanceName = configuration["Redis:InstanceName"];
            });


            // Typed HttpClient with Polly Resiliency
            services.AddHttpClient<HackerNewsHttpClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = opts.Timeout;
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Register API client abstraction
            services.AddSingleton<IHackerNewsHttpClient, HackerNewsHttpClient>();

            // Register API client abstraction
            services.AddSingleton<IHackerNewsApiClient, HackerNewsApiClient>();

            return services;
        }

        /// <summary>
        /// Retry policy: exponential backoff
        /// </summary>
        /// <returns>The <see cref="IAsyncPolicy{HttpResponseMessage}"/></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        /// <summary>
        /// The Circuit breaker: trip after 5 consecutive failures
        /// </summary>
        /// <returns>The <see cref="IAsyncPolicy{HttpResponseMessage}"/></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
