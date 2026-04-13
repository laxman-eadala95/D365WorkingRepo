namespace D365.Plugins.Common.Constants
{
    /// <summary>
    /// Late-bound metadata for the Contact entity and duplicate-email validation.
    /// </summary>
    public static class ContactConstants
    {
        public const string EntityLogicalName = "contact";

        public const string AttributeEmailAddress1 = "emailaddress1";

        public const string AttributeContactId = "contactid";

        public const string AttributeFirstName = "firstname";

        public const string AttributeLastName = "lastname";

        public const string AttributeParentCustomerId = "parentcustomerid";

        /// <summary>
        /// Default first name for the child contact created from an account.
        /// </summary>
        public const string DefaultFirstName = "Default";

        /// <summary>
        /// User-visible error when a duplicate email is detected on create.
        /// </summary>
        public const string DuplicateEmailMessage =
            "A contact with this email address already exists.";
    }
}
