using System;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Fetches new orders from Dataverse and sends each one to the external API.
    /// </summary>
    public class OrderSyncService
    {
        private readonly IOrderRepository _repository;
        private readonly IExternalApiClient _apiClient;
        private readonly Action<string> _log;

        public OrderSyncService(IOrderRepository repository, IExternalApiClient apiClient, Action<string> log)
        {
            _repository = repository;
            _apiClient = apiClient;
            _log = log ?? (_ => { });
        }

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
