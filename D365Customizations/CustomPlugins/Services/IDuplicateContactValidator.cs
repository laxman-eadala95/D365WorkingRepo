/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Contract for email-based duplicate detection against the Contact entity. Refer to following steps
**     1. EmailExists checks for an existing contact with the same email
**     2. ValidateNoDuplicateEmail throws InvalidPluginExecutionException when duplicate
*/

namespace CustomPlugins.Services
{
    /// <summary>
    /// Validates that a contact email is not already in use by another contact.
    /// </summary>
    public interface IDuplicateContactValidator
    {
        /// <summary>
        /// Returns whether any contact exists with the given email (case-sensitive match per platform query).
        /// </summary>
        /// <param name="email">Email address to check.</param>
        bool EmailExists(string email);

        /// <summary>
        /// Throws <see cref="InvalidPluginExecutionException"/> with a user message if the email is duplicate;
        /// no-ops when email is null or whitespace.
        /// </summary>
        /// <param name="email">Email from the creating contact.</param>
        void ValidateNoDuplicateEmail(string email);
    }
}
