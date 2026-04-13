using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace D365.Integration.OrderSync.Services
{
    public interface IOrderRepository
    {
        IList<Entity> GetOrdersCreatedSince(DateTime since);
    }
}
