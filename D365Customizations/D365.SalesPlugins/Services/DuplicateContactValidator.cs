/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Queries Contact by email with TopCount 1 to detect duplicates before save. Refer to following steps
**     1. Build QueryExpression on emailaddress1 with minimal ColumnSet
**     2. EmailExists returns whether any row matched; ValidateNoDuplicateEmail throws if duplicate and email not blank
*/

using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace D365.SalesPlugins.Services
{
    /// <summary>
    /// Checks whether a contact with the given email already exists in Dataverse.
    /// Uses QueryExpression.TopCount = 1 and a single-column ColumnSet for efficiency.
    /// </summary>
    public class DuplicateContactValidator : IDuplicateContactValidator
    {
        private readonly IOrganizationService _service;

        /// <summary>
        /// Initializes a new instance of the DuplicateContactValidator class.
        /// </summary>
        /// <param name="service">Organization service used for RetrieveMultiple.</param>
        public DuplicateContactValidator(IOrganizationService service)
        {
            _service = service;
        }

        /// <summary>
        /// This email check the email existing on Contact entity
        /// </summary>
        /// <param name="email">EmailID to check</param>
        /// <returns>True if email exisits else False</returns>
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

        /// <summary>
        /// Throw duplicate email error. On Default Child Case creation post Account creation the email will be empty. So, skipped the validation
        /// </summary>
        /// <param name="email">Email ID to validate</param>
        /// <exception cref="InvalidPluginExecutionException">Throw email already exists if it is duplicate</exception>
        public void ValidateNoDuplicateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            if (EmailExists(email))
                throw new InvalidPluginExecutionException(ContactConstants.DuplicateEmailMessage);
        }
    }
}
