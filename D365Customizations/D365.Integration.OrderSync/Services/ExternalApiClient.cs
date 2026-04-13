using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using Newtonsoft.Json;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Production client: POST JSON to a configured base URL (HTTPS recommended).
    /// </summary>
    public sealed class ExternalApiClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _ordersEndpoint;

        public ExternalApiClient(HttpClient httpClient, string ordersEndpoint)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ordersEndpoint = ordersEndpoint ?? throw new ArgumentNullException(nameof(ordersEndpoint));
        }

        /// <inheritdoc />
        public async Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            try
            {
                var json = JsonConvert.SerializeObject(payload);
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var response = await _httpClient.PostAsync(_ordersEndpoint, content).ConfigureAwait(false);
                    var body = response.Content != null ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty;
                    if (response.IsSuccessStatusCode)
                    {
                        return new ApiResponse
                        {
                            IsSuccess = true,
                            StatusCode = (int)response.StatusCode,
                            ErrorMessage = null
                        };
                    }

                    return new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = (int)response.StatusCode,
                        ErrorMessage = string.IsNullOrEmpty(body) ? response.ReasonPhrase : body
                    };
                }
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
