/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Retrieves salesorder rows from Dataverse using QueryExpression with createdon filter. Refer to following steps
**     1. Build QueryExpression on salesorder with required ColumnSet
**     2. Filter Condition GreaterEqual on createdon versus since parameter
**     3. Return RetrieveMultiple.Entities as IList
*/

using System;
using System.Collections.Generic;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Retrieves sales orders from Dataverse using QueryExpression with a created-on filter.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly IOrganizationService _service;

        /// <summary>
        /// Initializes a new instance of the OrderRepository class.
        /// </summary>
        /// <param name="service">Authenticated organization service (e.g. <c>ServiceClient</c>).</param>
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
                    OrderConstants.AttributeCreatedOn),
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
