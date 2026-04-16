/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Contract for sending order payloads to an external system and returning ApiResponse. Refer to following steps
**     1. SendOrderAsync posts OrderDetailsPayload and returns normalized result for logging
*/

using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Sends OrderDetailsPayload to an external system and returns a normalized ApiResponse.
    /// </summary>
    public interface IExternalApiClient
    {
        /// <summary>
        /// POSTs JSON representing the order; does not throw on HTTP error status (see ApiResponse).
        /// </summary>
        /// <param name="payload">Mapped sales order data.</param>
        Task<ApiResponse> SendOrderAsync(OrderDetailsPayload payload);
    }
}
