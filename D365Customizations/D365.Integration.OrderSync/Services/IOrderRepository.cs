/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Abstraction for querying sales orders created on or after a UTC timestamp. Refer to following steps
**     1. GetOrdersCreatedSince returns late-bound entities for the sync pipeline
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Retrieves sales order entities for the OrderSync pipeline.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Returns all sales orders whose created-on is greater than or equal to <paramref name="since"/>.
        /// </summary>
        /// <param name="since">Inclusive filter on <c>createdon</c>.</param>
        /// <returns>Late-bound entities with columns defined by the implementation.</returns>
        IList<Entity> GetOrdersCreatedSince(DateTime since);
    }
}
