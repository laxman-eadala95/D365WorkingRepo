using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Sends order payloads to the external REST API.
    /// </summary>
    public interface IExternalApiClient
    {
        Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload);
    }
}
