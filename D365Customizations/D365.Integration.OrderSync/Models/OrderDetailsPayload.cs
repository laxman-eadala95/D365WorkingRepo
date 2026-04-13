using System;

namespace D365.Integration.OrderSync.Models
{
    /// <summary>
    /// JSON payload sent to the external order API (maps from <c>salesorder</c>).
    /// </summary>
    public sealed class OrderDetailsPayload
    {
        public Guid? SalesOrderId { get; set; }

        /// <summary>
        /// Mapped from order <c>name</c> (assessment: customer-facing label on the order record).
        /// </summary>
        public string CustomerName { get; set; }

        public decimal? OrderTotal { get; set; }

        public DateTime? OrderDate { get; set; }
    }
}
