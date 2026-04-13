using System;
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
        public async Task Http200_ReturnsSuccess()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            });
            var client = new ExternalApiClient(new HttpClient(handler), "https://test/api/orders");

            var result = await client.SendOrderAsync(new OrderDetailsPayload());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Http500_ReturnsFailure()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(string.Empty)
            });
            var client = new ExternalApiClient(new HttpClient(handler), "https://test/api/orders");

            var result = await client.SendOrderAsync(new OrderDetailsPayload());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        // Returns a canned HTTP response for testing
        private class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                _respond = respond;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_respond(request));
            }
        }
    }
}
