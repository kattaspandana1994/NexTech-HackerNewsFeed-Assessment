namespace NexTech.HackerNews.Feed.Api.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System.Net;
    using System.Text.Json;

    namespace NexTech.HackerNews.Feed.Api.Middlewares
    {
        /// <summary>
        /// Defines the <see cref="ExceptionMiddleware" />
        /// </summary>
        public class ExceptionMiddleware
        {
            /// <summary>
            /// Defines the _next
            /// </summary>
            private readonly RequestDelegate _next;

            /// <summary>
            /// Defines the _logger
            /// </summary>
            private readonly ILogger<ExceptionMiddleware> _logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
            /// </summary>
            /// <param name="next">The next<see cref="RequestDelegate"/></param>
            /// <param name="logger">The logger<see cref="ILogger{ExceptionMiddleware}"/></param>
            public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            /// <summary>
            /// The InvokeAsync
            /// </summary>
            /// <param name="context">The context<see cref="HttpContext"/></param>
            /// <returns>The <see cref="Task"/></returns>
            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context); // continue down the pipeline
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred while processing the request.");
                    await HandleExceptionAsync(context, ex);
                }
            }

            /// <summary>
            /// The HandleExceptionAsync
            /// </summary>
            /// <param name="context">The context<see cref="HttpContext"/></param>
            /// <param name="ex">The ex<see cref="Exception"/></param>
            /// <returns>The <see cref="Task"/></returns>
            private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                var problem = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message,
                    Detail = ex.InnerException?.Message ?? "An unexpected error occurred."
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
            }
        }
    }
}
