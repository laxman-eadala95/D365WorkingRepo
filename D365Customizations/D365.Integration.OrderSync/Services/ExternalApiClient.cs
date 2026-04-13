/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Posts OrderDetailsPayload as JSON to a REST endpoint and maps HttpResponse to ApiResponse. Refer to following steps
**     1. Serialize payload with Newtonsoft.Json and POST to configured endpoint
**     2. Map success flag, status code, and error body; catch HttpRequestException for network failures
*/

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using Newtonsoft.Json;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Posts order payloads as JSON to a configured REST endpoint using <see cref="HttpClient"/>.
    /// </summary>
    public class ExternalApiClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalApiClient"/> class.
        /// </summary>
        /// <param name="httpClient">HTTP client instance (base address optional; <paramref name="endpoint"/> is absolute or relative).</param>
        /// <param name="endpoint">Full URL of the POST resource.</param>
        public ExternalApiClient(HttpClient httpClient, string endpoint)
        {
            _httpClient = httpClient;
            _endpoint = endpoint;
        }

        /// <inheritdoc />
        public async Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload)
        {
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_endpoint, content).ConfigureAwait(false);
                var body = response.Content != null
                    ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                    : string.Empty;

                return new ApiResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? null : body
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
