using System;
using CustomPlugins.Services;
using D365.Plugins.Common.Base;
using D365.Plugins.Common.Constants;

namespace CustomPlugins
{
    /// <summary>
    /// Registered on Account Create (Post-operation).
    /// Automatically creates a child Contact with first name "Default" and last name from the account.
    /// </summary>
    public class CreateChildContactOnAccountCreate : PluginBase
    {
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
