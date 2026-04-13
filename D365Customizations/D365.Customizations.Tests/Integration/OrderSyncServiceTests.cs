using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Models;
using D365.Integration.OrderSync.Services;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace D365.Customizations.Tests.Integration
{
    public class OrderSyncServiceTests
    {
        [Fact]
        public async Task NoOrders_LogsNoWorkMessage()
        {
            var repo = new StubOrderRepository(new List<Entity>());
            var api = new MockExternalApiClient(true);
            var logs = new List<string>();

            var sync = new OrderSyncService(repo, api, logs.Add);
            await sync.SyncNewOrdersAsync(DateTime.UtcNow);

            Assert.Single(logs);
            Assert.Contains("No orders found", logs[0]);
        }

        [Fact]
        public async Task ValidOrder_LogsSuccess()
        {
            var order = new Entity(OrderConstants.EntityLogicalName, Guid.NewGuid())
            {
                [OrderConstants.AttributeName] = "SO-1",
                [OrderConstants.AttributeTotalAmount] = new Money(10m),
                [OrderConstants.AttributeCreatedOn] = DateTime.UtcNow
            };

            var repo = new StubOrderRepository(new List<Entity> { order });
            var api = new MockExternalApiClient(true);
            var logs = new List<string>();

            var sync = new OrderSyncService(repo, api, logs.Add);
            await sync.SyncNewOrdersAsync(DateTime.UtcNow.AddDays(-1));

            Assert.Contains("SUCCESS", logs[0]);
        }

        [Fact]
        public void MapToPayload_MapsFieldsCorrectly()
        {
            var id = Guid.NewGuid();
            var order = new Entity(OrderConstants.EntityLogicalName, id)
            {
                [OrderConstants.AttributeName] = "Test",
                [OrderConstants.AttributeTotalAmount] = new Money(42m),
                [OrderConstants.AttributeCreatedOn] = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var payload = OrderSyncService.MapToPayload(order);

            Assert.Equal("Test", payload.CustomerName);
            Assert.Equal(42m, payload.OrderTotal);
            Assert.Equal(new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), payload.OrderDate);
        }

        // Simple stub for tests - just returns what you give it
        private class StubOrderRepository : IOrderRepository
        {
            private readonly IList<Entity> _orders;

            public StubOrderRepository(IList<Entity> orders)
            {
                _orders = orders;
            }

            public IList<Entity> GetOrdersCreatedSince(DateTime since) => _orders;
        }
    }
}
