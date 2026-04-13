/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Centralizes Account entity logical names and attribute constants for plugins and services. Refer to following steps
**     1. Define entity logical name and primary key attribute
**     2. Expose name attribute used when creating child records from account data
*/

namespace D365.Plugins.Common.Constants
{
    /// <summary>
    /// Account entity field names used across plugins and services.
    /// </summary>
    public static class AccountConstants
    {
        /// <summary>Logical name of the Account entity.</summary>
        public const string EntityLogicalName = "account";

        /// <summary>Primary key attribute logical name.</summary>
        public const string AttributeAccountId = "accountid";

        /// <summary>Account name attribute (used as source for child contact last name).</summary>
        public const string AttributeName = "name";
    }
}
