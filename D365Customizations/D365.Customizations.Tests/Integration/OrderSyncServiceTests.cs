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
        public async Task TC_I07_NoOrders_LogsOnly()
        {
            var repo = new MockOrderRepository(new List<Entity>());
            var api = new MockExternalApiClient(true);
            var logs = new List<string>();
            var s = new OrderSyncService(repo, api, logs.Add);
            await s.SyncNewOrdersAsync(DateTime.UtcNow).ConfigureAwait(false);
            Assert.Single(logs);
            Assert.Contains("No orders found", logs[0]);
        }

        [Fact]
        public async Task TC_I01_SuccessPath()
        {
            var order = new Entity(OrderConstants.EntityLogicalName, Guid.NewGuid())
            {
                [OrderConstants.AttributeName] = "SO-1",
                [OrderConstants.AttributeTotalAmount] = new Money(10m),
                [OrderConstants.AttributeCreatedOn] = DateTime.UtcNow
            };
            var repo = new MockOrderRepository(new List<Entity> { order });
            var api = new MockExternalApiClient(true);
            var logs = new List<string>();
            var s = new OrderSyncService(repo, api, logs.Add);
            await s.SyncNewOrdersAsync(DateTime.UtcNow.AddDays(-1)).ConfigureAwait(false);
            Assert.Contains("SUCCESS", logs[0]);
        }

        [Fact]
        public void TC_I02_PayloadMapping()
        {
            var id = Guid.NewGuid();
            var order = new Entity(OrderConstants.EntityLogicalName, id)
            {
                [OrderConstants.AttributeName] = "N",
                [OrderConstants.AttributeTotalAmount] = new Money(5m),
                [OrderConstants.AttributeCreatedOn] = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            };
            var p = OrderSyncService.MapToPayload(order);
            Assert.Equal("N", p.CustomerName);
            Assert.Equal(5m, p.OrderTotal);
            Assert.Equal(new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc), p.OrderDate);
        }

        private sealed class MockOrderRepository : IOrderRepository
        {
            private readonly IList<Entity> _entities;

            public MockOrderRepository(IList<Entity> entities)
            {
                _entities = entities;
            }

            public IList<Entity> GetOrdersCreatedSince(DateTime since)
            {
                return _entities;
            }
        }
    }
}
