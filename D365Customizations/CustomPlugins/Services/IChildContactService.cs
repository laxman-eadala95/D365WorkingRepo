using System;

namespace CustomPlugins.Services
{
    /// <summary>
    /// Creates a child contact linked to an account.
    /// </summary>
    public interface IChildContactService
    {
        /// <summary>
        /// Creates a contact with default first name and account name as last name.
        /// </summary>
        Guid CreateChildContact(Guid accountId, string accountName);
    }
}
