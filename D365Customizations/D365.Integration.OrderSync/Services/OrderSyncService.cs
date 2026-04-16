/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Orchestrates fetching orders from Dataverse and posting each payload to the external API with per-order logging. Refer to following steps
**     1. Get orders from IOrderRepository for createdon since the given timestamp
**     2. Map each Entity to OrderDetailsPayload and await IExternalApiClient.SendOrderAsync
**     3. Log SUCCESS or FAILURE lines including status code and error text when failed
*/

using System;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Fetches sales orders created on or after a timestamp and sends each mapped payload to IExternalApiClient.
    /// </summary>
    public class OrderSyncService
    {
        private readonly IOrderRepository _repository;
        private readonly IExternalApiClient _apiClient;
        private readonly Action<string> _log;

        /// <summary>
        /// Initializes a new instance of the OrderSyncService class.
        /// </summary>
        /// <param name="repository">Dataverse access for orders in the sync window.</param>
        /// <param name="apiClient">Outbound HTTP (or mock) client.</param>
        /// <param name="log">Optional logger; defaults to no-op when null.</param>
        public OrderSyncService(IOrderRepository repository, IExternalApiClient apiClient, Action<string> log)
        {
            _repository = repository;
            _apiClient = apiClient;
            _log = log ?? (_ => { });
        }

        /// <summary>
        /// Retrieves orders with createdon >= since,
        /// maps each to OrderDetailsPayload, and awaits IExternalApiClient.SendOrderAsync.
        /// </summary>
        /// <param name="since">Inclusive lower bound (UTC recommended).</param>
        public async Task SyncNewOrdersAsync(DateTime since)
        {
            var orders = _repository.GetOrdersCreatedSince(since);

            if (orders == null || orders.Count == 0)
            {
                _log($"[{DateTime.UtcNow:O}] No orders found since {since:O}.");
                return;
            }

            foreach (var order in orders)
            {
                var payload = MapToPayload(order);
                var result = await _apiClient.SendOrderAsync(payload).ConfigureAwait(false);

                if (result.IsSuccess)
                    _log($"[{DateTime.UtcNow:O}] SUCCESS order={payload.SalesOrderId} status={result.StatusCode}");
                else
                    _log($"[{DateTime.UtcNow:O}] FAILURE order={payload.SalesOrderId} status={result.StatusCode} error={result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Maps a late-bound sales order Entity to the contract expected by the external API.
        /// </summary>
        /// <param name="order">Record from IOrderRepository.GetOrdersCreatedSince.</param>
        /// <returns>Payload containing id, display name as customer label, total, and created date.</returns>
        public static OrderDetailsPayload MapToPayload(Entity order)
        {
            return new OrderDetailsPayload
            {
                SalesOrderId = order.Id != Guid.Empty ? order.Id : order.GetAttributeValue<Guid?>(OrderConstants.AttributeSalesOrderId),
                CustomerName = order.GetAttributeValue<string>(OrderConstants.AttributeName),
                OrderTotal = order.GetAttributeValue<Money>(OrderConstants.AttributeTotalAmount)?.Value,
                OrderDate = order.GetAttributeValue<DateTime?>(OrderConstants.AttributeCreatedOn)
            };
        }
    }
}
