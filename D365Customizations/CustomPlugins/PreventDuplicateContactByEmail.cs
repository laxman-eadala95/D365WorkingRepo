using System;
using CustomPlugins.Services;
using D365.Plugins.Common.Base;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace CustomPlugins
{
    /// <summary>
    /// Pre-validation: blocks duplicate <c>emailaddress1</c> values using <see cref="DuplicateContactValidator"/>.
    /// </summary>
    public sealed class PreventDuplicateContactByEmail : PluginBase
    {
        /// <inheritdoc />
        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }

            if (!localContext.Target.LogicalName.Equals(
                    ContactConstants.EntityLogicalName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var email = localContext.Target.GetAttributeValue<string>(ContactConstants.AttributeEmailAddress1);
            var validator = new DuplicateContactValidator(localContext.Service);
            validator.ValidateNoDuplicateEmail(email);
        }
    }
}
