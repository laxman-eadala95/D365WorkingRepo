using CustomPlugins.Services;
using D365.Plugins.Common.Base;
using D365.Plugins.Common.Constants;

namespace CustomPlugins
{
    /// <summary>
    /// Registered on Contact Create (Pre-validation).
    /// Blocks the create if another contact already has the same email address.
    /// </summary>
    public class PreventDuplicateContactByEmail : PluginBase
    {
        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            if (localContext.Target.LogicalName != ContactConstants.EntityLogicalName)
                return;

            var email = localContext.Target.GetAttributeValue<string>(ContactConstants.AttributeEmailAddress1);

            var validator = new DuplicateContactValidator(localContext.Service);
            validator.ValidateNoDuplicateEmail(email);
        }
    }
}
