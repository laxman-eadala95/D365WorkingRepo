using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace D365.Integration.OrderSync.Services
{
    /// <summary>
    /// Retrieves sales orders from Dataverse for outbound sync.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Returns orders with <c>createdon</c> greater than or equal to <paramref name="since"/> (UTC).
        /// </summary>
        IList<Entity> GetOrdersCreatedSince(DateTime since);
    }
}
