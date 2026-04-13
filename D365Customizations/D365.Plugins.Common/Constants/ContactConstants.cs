/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Centralizes Contact schema names and user-facing strings for duplicate validation and child contact creation. Refer to following steps
**     1. Define entity, key, email, name, and parent lookup logical names
**     2. Hold default first name and duplicate-email error message for plugins
*/

namespace D365.Plugins.Common.Constants
{
    /// <summary>
    /// Contact entity field names and shared values used across plugins and services.
    /// </summary>
    public static class ContactConstants
    {
        /// <summary>Logical name of the Contact entity.</summary>
        public const string EntityLogicalName = "contact";

        /// <summary>Primary email attribute used for duplicate detection.</summary>
        public const string AttributeEmailAddress1 = "emailaddress1";

        /// <summary>Primary key attribute logical name.</summary>
        public const string AttributeContactId = "contactid";

        /// <summary>First name attribute.</summary>
        public const string AttributeFirstName = "firstname";

        /// <summary>Last name attribute.</summary>
        public const string AttributeLastName = "lastname";

        /// <summary>Parent customer lookup (Account or Contact).</summary>
        public const string AttributeParentCustomerId = "parentcustomerid";

        /// <summary>Default first name when auto-creating a child contact from an account.</summary>
        public const string DefaultFirstName = "Default";

        /// <summary>Message thrown when a create is blocked due to an existing contact with the same email.</summary>
        public const string DuplicateEmailMessage = "A contact with this email address already exists.";
    }
}
