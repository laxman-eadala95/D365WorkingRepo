using System;
using CustomPlugins.Services;
using D365.Plugins.Common.Base;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace CustomPlugins
{
    /// <summary>
    /// Post-operation on account create: adds a child contact with first name Default and last name from the account.
    /// </summary>
    public sealed class CreateChildContactOnAccountCreate : PluginBase
    {
        /// <inheritdoc />
        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }

            if (!localContext.Target.LogicalName.Equals(
                    AccountConstants.EntityLogicalName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var accountId = localContext.Context.PrimaryEntityId;
            if (accountId == Guid.Empty)
            {
                return;
            }

            var accountName = localContext.Target.GetAttributeValue<string>(AccountConstants.AttributeName);
            var service = new ChildContactService(localContext.Service, localContext.TracingService);
            service.CreateChildContact(accountId, accountName);
        }
    }
}
