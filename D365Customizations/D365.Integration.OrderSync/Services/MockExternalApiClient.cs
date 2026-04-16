/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: In-memory IExternalApiClient for tests and local runs without HTTP. Refer to following steps
**     1. Return success or failure ApiResponse from constructor flag via Task.FromResult
*/

using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Returns canned ApiResponse values without HTTP calls. Used for local runs and unit tests.
    /// </summary>
    public class MockExternalApiClient : IExternalApiClient
    {
        private readonly bool _succeed;

        /// <summary>
        /// Initializes a new instance of the MockExternalApiClient class.
        /// </summary>
        /// <param name="succeed">If true, simulates HTTP 200; otherwise simulates a server error.</param>
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
