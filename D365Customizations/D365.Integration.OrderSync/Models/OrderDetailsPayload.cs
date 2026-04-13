using System;

namespace D365.Integration.OrderSync.Models
{
    /// <summary>
    /// Payload sent to the external order API. Mapped from the salesorder entity.
    /// </summary>
    public class OrderDetailsPayload
    {
        public Guid? SalesOrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal? OrderTotal { get; set; }
        public DateTime? OrderDate { get; set; }
    }
}
