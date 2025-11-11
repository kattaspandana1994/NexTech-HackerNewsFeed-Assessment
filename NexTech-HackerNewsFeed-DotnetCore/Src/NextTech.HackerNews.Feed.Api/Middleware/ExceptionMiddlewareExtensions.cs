namespace NexTech.HackerNews.Feed.Api.Middleware
{
    using Microsoft.AspNetCore.Builder;

    namespace NexTech.HackerNews.Feed.Api.Middlewares
    {
        public static class ExceptionMiddlewareExtensions
        {
            public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder app)
            {
                return app.UseMiddleware<ExceptionMiddleware>();
            }
        }
    }

}
