using System;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CustomPlugins.Services
{
    /// <summary>
    /// Uses a narrow query (TopCount = 1, single column) for scalable duplicate detection.
    /// </summary>
    public sealed class DuplicateContactValidator : IDuplicateContactValidator
    {
        private readonly IOrganizationService _service;

        public DuplicateContactValidator(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public bool EmailExists(string email)
        {
            var query = new QueryExpression(ContactConstants.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(ContactConstants.AttributeContactId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            ContactConstants.AttributeEmailAddress1,
                            ConditionOperator.Equal,
                            email)
                    }
                },
                TopCount = 1
            };

            var results = _service.RetrieveMultiple(query);
            return results.Entities.Count > 0;
        }

        /// <inheritdoc />
        public void ValidateNoDuplicateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            if (EmailExists(email))
            {
                throw new InvalidPluginExecutionException(ContactConstants.DuplicateEmailMessage);
            }
        }
    }
}
