using System.Net;

namespace NexTech.HackerNews.Feed.Infrastructure.Tests.TestUtilities
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage>? responseFactory = null)
        {
            _responseFactory = responseFactory ?? (_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}") // default empty JSON
            });
        }

        public MockHttpMessageHandler(HttpStatusCode statusCode)
        {
            _responseFactory = _ => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("{}") // always give valid content
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Always return a fresh HttpResponseMessage
            var response = _responseFactory(request);
            return Task.FromResult(response);
        }
    }
}
