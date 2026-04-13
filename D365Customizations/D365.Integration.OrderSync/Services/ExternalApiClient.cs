using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using Newtonsoft.Json;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Posts order payloads as JSON to a configured REST endpoint.
    /// </summary>
    public class ExternalApiClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public ExternalApiClient(HttpClient httpClient, string endpoint)
        {
            _httpClient = httpClient;
            _endpoint = endpoint;
        }

        public async Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload)
        {
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_endpoint, content).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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
