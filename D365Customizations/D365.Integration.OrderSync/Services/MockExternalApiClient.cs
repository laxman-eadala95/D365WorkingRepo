using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// In-process mock for demos and tests (no network).
    /// </summary>
    public sealed class MockExternalApiClient : IExternalApiClient
    {
        private readonly bool _succeed;

        public MockExternalApiClient(bool succeed = true)
        {
            _succeed = succeed;
        }

        /// <inheritdoc />
        public Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload)
        {
            var response = _succeed
                ? new ApiResponse { IsSuccess = true, StatusCode = 200, ErrorMessage = null }
                : new ApiResponse { IsSuccess = false, StatusCode = 500, ErrorMessage = "Mock failure" };
            return Task.FromResult(response);
        }
    }
}
