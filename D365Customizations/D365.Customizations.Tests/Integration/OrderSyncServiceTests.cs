/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Tests OrderSyncService sync loop, logging, MapToPayload, and StubOrderRepository. Refer to following steps
**     1. Empty orders logs no work; single order with mock API logs SUCCESS
**     2. MapToPayload maps name, Money total, and createdon
*/

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
    /// <summary>
    /// Tests for OrderSyncService sync loop and static mapping helper.
    /// </summary>
    public class OrderSyncServiceTests
    {
        /// <summary>Empty repository should log that no orders were found.</summary>
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

        /// <summary>Single order and successful API should produce a SUCCESS log line.</summary>
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

        /// <summary>MapToPayload should copy name, money value, and created-on.</summary>
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

        /// <summary>In-memory repository stub returning a fixed list for tests.</summary>
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
