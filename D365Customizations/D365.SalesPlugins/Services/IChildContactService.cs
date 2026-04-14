/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Interface for creating a Contact child of an Account to support testing and dependency injection. Refer to following steps
**     1. Define CreateChildContact with account id and display name used for last name
*/

using System;

namespace D365.SalesPlugins.Services
{
    /// <summary>
    /// Defines creation of a contact linked to an account with default naming rules.
    /// </summary>
    public interface IChildContactService
    {
        /// <summary>
        /// Creates a contact with parent set to the given account and writes trace output on success.
        /// </summary>
        /// <param name="accountId">Primary key of the parent account.</param>
        /// <param name="accountName">Value used for the contact last name.</param>
        /// <returns>The new contact's identifier.</returns>
        Guid CreateChildContact(Guid accountId, string accountName);
    }
}
