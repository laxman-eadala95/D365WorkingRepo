/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Plugin registered on Account Create (Post-operation) that creates a default child Contact linked to the new account. Refer to following steps
**     1. Verify Target is account and PrimaryEntityId is not empty
**     2. Read account name and delegate to ChildContactService to create the contact
*/

using System;
using CustomPlugins.Services;
using D365.Plugins.Common.Base;
using D365.Plugins.Common.Constants;

namespace CustomPlugins
{
    /// <summary>
    /// Registered on Account Create (Post-operation).
    /// Creates a child <see cref="ContactConstants.EntityLogicalName"/> with first name
    /// <see cref="ContactConstants.DefaultFirstName"/> and last name taken from the account name.
    /// </summary>
    public class CreateChildContactOnAccountCreate : PluginBase
    {
        /// <inheritdoc />
        /// <remarks>
        /// Ignores non-account targets, empty primary id, and delegates creation to <see cref="ChildContactService"/>.
        /// </remarks>
        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            if (localContext.Target.LogicalName != AccountConstants.EntityLogicalName)
                return;

            var accountId = localContext.Context.PrimaryEntityId;
            if (accountId == Guid.Empty)
                return;

            var accountName = localContext.Target.GetAttributeValue<string>(AccountConstants.AttributeName);

            var service = new ChildContactService(localContext.Service, localContext.TracingService);
            service.CreateChildContact(accountId, accountName);
        }
    }
}
