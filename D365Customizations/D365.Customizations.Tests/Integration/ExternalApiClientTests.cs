using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using D365.Integration.OrderSync.Services;
using Xunit;

namespace D365.Customizations.Tests.Integration
{
    public class ExternalApiClientTests
    {
        [Fact]
        public async Task TC_I09_Http200_Success()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var http = new HttpClient(handler);
            var client = new ExternalApiClient(http, "https://example.test/api/orders");
            var r = await client.SendOrderAsync(new OrderDetailsPayload()).ConfigureAwait(false);
            Assert.True(r.IsSuccess);
            Assert.Equal(200, r.StatusCode);
        }

        [Fact]
        public async Task TC_I10_Http500_Failure()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var http = new HttpClient(handler);
            var client = new ExternalApiClient(http, "https://example.test/api/orders");
            var r = await client.SendOrderAsync(new OrderDetailsPayload()).ConfigureAwait(false);
            Assert.False(r.IsSuccess);
            Assert.Equal(500, r.StatusCode);
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly System.Func<HttpRequestMessage, HttpResponseMessage> _respond;

            public StubHandler(System.Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                _respond = respond;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_respond(request));
            }
        }
    }
}
