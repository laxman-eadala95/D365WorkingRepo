using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Returns canned responses without making any HTTP calls. For demos and tests.
    /// </summary>
    public class MockExternalApiClient : IExternalApiClient
    {
        private readonly bool _succeed;

        public MockExternalApiClient(bool succeed = true)
        {
            _succeed = succeed;
        }

        public Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload)
        {
            var response = _succeed
                ? new ApiResponse { IsSuccess = true, StatusCode = 200 }
                : new ApiResponse { IsSuccess = false, StatusCode = 500, ErrorMessage = "Mock failure" };

            return Task.FromResult(response);
        }
    }
}
