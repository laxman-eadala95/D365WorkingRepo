using System;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Orchestrates retrieval of new orders and POST to the external API with structured logging.
    /// </summary>
    public sealed class OrderSyncService
    {
        private readonly IOrderRepository _repository;
        private readonly IExternalApiClient _apiClient;
        private readonly Action<string> _log;

        public OrderSyncService(
            IOrderRepository repository,
            IExternalApiClient apiClient,
            Action<string> log)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _log = log ?? (_ => { });
        }

        /// <summary>
        /// Processes all orders created on or after <paramref name="since"/> (UTC).
        /// </summary>
        public async Task SyncNewOrdersAsync(DateTime since)
        {
            var orders = _repository.GetOrdersCreatedSince(since);
            if (orders == null || orders.Count == 0)
            {
                _log($"[{DateTime.UtcNow:O}] No orders found since {since:O}.");
                return;
            }

            foreach (var entity in orders)
            {
                var payload = MapToPayload(entity);
                var result = await _apiClient.SendOrderAsync(payload).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    _log(
                        $"[{DateTime.UtcNow:O}] SUCCESS order={payload.SalesOrderId} status={result.StatusCode} customerName={payload.CustomerName}");
                }
                else
                {
                    _log(
                        $"[{DateTime.UtcNow:O}] FAILURE order={payload.SalesOrderId} status={result.StatusCode} error={result.ErrorMessage}");
                }
            }
        }

        public static OrderDetailsPayload MapToPayload(Entity order)
        {
            var id = order.Id != Guid.Empty
                ? order.Id
                : order.GetAttributeValue<Guid?>(OrderConstants.AttributeSalesOrderId);

            return new OrderDetailsPayload
            {
                SalesOrderId = id,
                CustomerName = order.GetAttributeValue<string>(OrderConstants.AttributeName),
                OrderTotal = order.GetAttributeValue<Money>(OrderConstants.AttributeTotalAmount)?.Value,
                OrderDate = order.GetAttributeValue<DateTime?>(OrderConstants.AttributeCreatedOn)
            };
        }
    }
}
