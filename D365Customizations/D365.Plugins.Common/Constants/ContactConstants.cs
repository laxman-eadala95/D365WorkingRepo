namespace D365.Plugins.Common.Constants
{
    /// <summary>
    /// Contact entity field names and shared values used across plugins and services.
    /// </summary>
    public static class ContactConstants
    {
        public const string EntityLogicalName = "contact";
        public const string AttributeEmailAddress1 = "emailaddress1";
        public const string AttributeContactId = "contactid";
        public const string AttributeFirstName = "firstname";
        public const string AttributeLastName = "lastname";
        public const string AttributeParentCustomerId = "parentcustomerid";

        public const string DefaultFirstName = "Default";
        public const string DuplicateEmailMessage = "A contact with this email address already exists.";
    }
}
