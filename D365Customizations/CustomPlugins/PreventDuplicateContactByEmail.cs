/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Plugin registered on Contact Create (Pre-validation) that blocks create when email already exists. Refer to following steps
**     1. Verify Target is contact and read emailaddress1 from Target
**     2. Call DuplicateContactValidator to query Dataverse and throw if duplicate
*/

using CustomPlugins.Services;
using D365.Plugins.Common.Base;
using D365.Plugins.Common.Constants;

namespace CustomPlugins
{
    /// <summary>
    /// Registered on Contact Create (Pre-validation).
    /// Throws <see cref="InvalidPluginExecutionException"/> if another contact already has the same email.
    /// </summary>
    public class PreventDuplicateContactByEmail : PluginBase
    {
        /// <inheritdoc />
        /// <remarks>
        /// Skips non-contact entities. Empty email is not validated (allows creates without email).
        /// </remarks>
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
