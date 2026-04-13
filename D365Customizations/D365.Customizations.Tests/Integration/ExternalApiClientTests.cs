/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Tests ExternalApiClient with Stub HttpMessageHandler for 200 and 500 responses without network. Refer to following steps
**     1. Assert ApiResponse IsSuccess and StatusCode for success and failure paths
*/

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
    /// <summary>
    /// Tests for <see cref="ExternalApiClient"/> success and error status mapping.
    /// </summary>
    public class ExternalApiClientTests
    {
        /// <summary>2xx response should yield IsSuccess and status code.</summary>
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

        /// <summary>5xx response should be treated as failure with status preserved.</summary>
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

        /// <summary>Test handler that returns a canned <see cref="HttpResponseMessage"/> per request.</summary>
        private class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            {
                _respond = respond;
            }

            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_respond(request));
            }
        }
    }
}
