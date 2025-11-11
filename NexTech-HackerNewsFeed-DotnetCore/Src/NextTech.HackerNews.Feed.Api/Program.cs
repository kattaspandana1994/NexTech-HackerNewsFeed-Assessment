namespace NexTech.HackerNews.Feed.Api
{
    using NexTech.HackerNews.Feed.Application.Interfaces;
    using NexTech.HackerNews.Feed.Infrastructure.Caching;
    using NexTech.HackerNews.Feed.Api.Middleware.NexTech.HackerNews.Feed.Api.Middlewares;
    using NexTech.HackerNews.Feed.Api.Workers;

    /// <summary>
    /// Defines the <see cref="Program" />
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The Main
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Services.AddLogging();
            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });
            builder.Services.Configure<CacheWarmerOptions>(
            builder.Configuration.GetSection("CacheWarmer"));
            builder.Services.AddHostedService<TopStoriesCacheWarmer>();
            NexTech.HackerNews.Feed.Infrastructure.DI.ServiceCollectionExtensions.AddInfrastructureAsync(builder.Services, builder.Configuration);
            Extensions.ServiceCollectionExtensions.AddHackerNewsServices(builder.Services, builder.Configuration);                   

            var app = builder.Build();

            app.UseCustomExceptionMiddleware();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
