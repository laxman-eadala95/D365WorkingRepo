using System;
using System.Collections.Generic;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Retrieves sales orders from Dataverse using QueryExpression (late-bound).
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly IOrganizationService _service;

        public OrderRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IList<Entity> GetOrdersCreatedSince(DateTime since)
        {
            var query = new QueryExpression(OrderConstants.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    OrderConstants.AttributeSalesOrderId,
                    OrderConstants.AttributeName,
                    OrderConstants.AttributeTotalAmount,
                    OrderConstants.AttributeCreatedOn,
                    OrderConstants.AttributeCustomerId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            OrderConstants.AttributeCreatedOn,
                            ConditionOperator.GreaterEqual,
                            since)
                    }
                }
            };

            return _service.RetrieveMultiple(query).Entities;
        }
    }
}
