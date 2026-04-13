using System;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace CustomPlugins.Services
{
    /// <summary>
    /// Creates a child contact linked to an account and writes a trace log on success.
    /// </summary>
    public class ChildContactService : IChildContactService
    {
        private readonly IOrganizationService _service;
        private readonly ITracingService _tracing;

        public ChildContactService(IOrganizationService service, ITracingService tracing)
        {
            _service = service;
            _tracing = tracing;
        }

        public Guid CreateChildContact(Guid accountId, string accountName)
        {
            var contact = new Entity(ContactConstants.EntityLogicalName)
            {
                [ContactConstants.AttributeFirstName] = ContactConstants.DefaultFirstName,
                [ContactConstants.AttributeLastName] = accountName,
                [ContactConstants.AttributeParentCustomerId] = new EntityReference(
                    AccountConstants.EntityLogicalName, accountId)
            };

            var contactId = _service.Create(contact);

            _tracing.Trace("Child contact {0} created for account '{1}' ({2}).",
                contactId, accountName ?? "(null)", accountId);

            return contactId;
        }
    }
}
