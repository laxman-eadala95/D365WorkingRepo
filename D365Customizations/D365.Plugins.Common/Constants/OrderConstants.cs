/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Sales Order (salesorder) attribute logical names for OrderSync integration and payload mapping. Refer to following steps
**     1. Define entity and columns used in QueryExpression and MapToPayload
**     2. Include createdon filter field used in sync window query
*/

namespace D365.Plugins.Common.Constants
{
    /// <summary>
    /// Sales Order entity field names used in the integration console app and payload mapping.
    /// </summary>
    public static class OrderConstants
    {
        /// <summary>Logical name of the Sales Order entity.</summary>
        public const string EntityLogicalName = "salesorder";

        /// <summary>Primary key attribute logical name.</summary>
        public const string AttributeSalesOrderId = "salesorderid";

        /// <summary>Order name / description (mapped to API payload customer-facing label).</summary>
        public const string AttributeName = "name";

        /// <summary>Order total amount (Money field).</summary>
        public const string AttributeTotalAmount = "totalamount";

        /// <summary>Record created-on timestamp (used as filter and payload order date).</summary>
        public const string AttributeCreatedOn = "createdon";
    }
}
