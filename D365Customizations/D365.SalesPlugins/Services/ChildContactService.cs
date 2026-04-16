/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Creates a child Contact for an Account with default naming and trace logging. Refer to following steps
**     1. Build Contact entity with first name, last name from account name, and parentcustomerid to account
**     2. Call IOrganizationService.Create and write trace with new id and account reference
*/

using System;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace D365.SalesPlugins.Services
{
    /// <summary>
    /// Creates a child contact linked to an account and writes a trace log on successful create.
    /// </summary>
    public class ChildContactService : IChildContactService
    {
        private readonly IOrganizationService _service;
        private readonly ITracingService _tracing;

        /// <summary>
        /// Initializes a new instance of the ChildContactService class.
        /// </summary>
        /// <param name="service">Organization service used to call IOrganizationService.Create.</param>
        /// <param name="tracing">Tracing service for post-create diagnostics.</param>
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
