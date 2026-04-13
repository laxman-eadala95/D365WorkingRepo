using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CustomPlugins.Services
{
    /// <summary>
    /// Checks whether a contact with the given email already exists in D365.
    /// Uses TopCount = 1 and a single-column ColumnSet for performance at scale.
    /// </summary>
    public class DuplicateContactValidator : IDuplicateContactValidator
    {
        private readonly IOrganizationService _service;

        public DuplicateContactValidator(IOrganizationService service)
        {
            _service = service;
        }

        public bool EmailExists(string email)
        {
            var query = new QueryExpression(ContactConstants.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(ContactConstants.AttributeContactId),
                TopCount = 1,
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            ContactConstants.AttributeEmailAddress1,
                            ConditionOperator.Equal,
                            email)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            return results.Entities.Count > 0;
        }

        public void ValidateNoDuplicateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            if (EmailExists(email))
                throw new InvalidPluginExecutionException(ContactConstants.DuplicateEmailMessage);
        }
    }
}
