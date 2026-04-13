namespace CustomPlugins.Services
{
    /// <summary>
    /// Validates that a contact email is not already used in the system.
    /// </summary>
    public interface IDuplicateContactValidator
    {
        /// <summary>
        /// Returns true if any contact exists with the given email.
        /// </summary>
        bool EmailExists(string email);

        /// <summary>
        /// Throws <see cref="Microsoft.Xrm.Sdk.InvalidPluginExecutionException"/> if the email is a duplicate.
        /// </summary>
        void ValidateNoDuplicateEmail(string email);
    }
}
