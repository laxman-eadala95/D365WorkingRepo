/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: DTO for JSON sent to the external order API; mapped from salesorder attributes. Refer to following steps
**     1. Carry SalesOrderId, CustomerName, OrderTotal, and OrderDate for downstream systems
*/

using System;

namespace D365.Integration.OrderSync.Models
{
    /// <summary>
    /// Payload sent to the external order API. Typically produced by OrderSyncService.MapToPayload.
    /// </summary>
    public class OrderDetailsPayload
    {
        /// <summary>Sales order unique identifier.</summary>
        public Guid? SalesOrderId { get; set; }

        /// <summary>Mapped from order name field for display in downstream systems.</summary>
        public string CustomerName { get; set; }

        /// <summary>Order total from Money attribute.</summary>
        public decimal? OrderTotal { get; set; }

        /// <summary>Created-on timestamp from Dataverse.</summary>
        public DateTime? OrderDate { get; set; }
    }
}
