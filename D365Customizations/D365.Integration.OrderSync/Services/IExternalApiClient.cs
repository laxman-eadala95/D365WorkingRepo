using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;

namespace D365.Integration.OrderSync.Services
{
    public interface IExternalApiClient
    {
        Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload);
    }
}
