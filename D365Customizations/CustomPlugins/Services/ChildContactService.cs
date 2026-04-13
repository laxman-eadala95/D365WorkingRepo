using System;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace CustomPlugins.Services
{
    /// <summary>
    /// Creates and persists the child contact; traces success for operations visibility.
    /// </summary>
    public sealed class ChildContactService : IChildContactService
    {
        private readonly IOrganizationService _service;
        private readonly ITracingService _tracing;

        public ChildContactService(IOrganizationService service, ITracingService tracing)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _tracing = tracing ?? throw new ArgumentNullException(nameof(tracing));
        }

        /// <inheritdoc />
        public Guid CreateChildContact(Guid accountId, string accountName)
        {
            var contact = new Entity(ContactConstants.EntityLogicalName)
            {
                [ContactConstants.AttributeFirstName] = ContactConstants.DefaultFirstName,
                [ContactConstants.AttributeLastName] = accountName,
                [ContactConstants.AttributeParentCustomerId] = new EntityReference(
                    AccountConstants.EntityLogicalName,
                    accountId)
            };

            var contactId = _service.Create(contact);
            _tracing.Trace(
                $"Child contact created for account '{accountName ?? "(null)"}' (account id: {accountId}, contact id: {contactId}).");
            return contactId;
        }
    }
}
