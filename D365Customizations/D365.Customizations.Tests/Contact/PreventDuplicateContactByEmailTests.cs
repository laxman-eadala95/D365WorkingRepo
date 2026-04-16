/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Tests PreventDuplicateContactByEmail plugin with mocked RetrieveMultiple for duplicate scenarios. Refer to following steps
**     1. Unique email completes without exception
**     2. Duplicate email throws InvalidPluginExecutionException
*/

using D365.SalesPlugins;
using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Contact
{
    /// <summary>
    /// Integration-style tests for PreventDuplicateContactByEmail against PluginMockFactory.
    /// </summary>
    public class PreventDuplicateContactByEmailTests
    {
        /// <summary>No matching contact should complete without exception.</summary>
        [Fact]
        public void UniqueEmail_NoException()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "unique@example.com";

            var mock = PluginMockFactory.Create(target, "Create", 10);
            mock.OrganizationService
                .Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection());

            new PreventDuplicateContactByEmail().Execute(mock.ServiceProvider.Object);
        }

        /// <summary>Existing contact with same email should throw before save.</summary>
        [Fact]
        public void DuplicateEmail_ThrowsInvalidPluginExecutionException()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "dup@example.com";

            var mock = PluginMockFactory.Create(target, "Create", 10);
            mock.OrganizationService
                .Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection(new[] { new Entity("contact") }));

            Assert.Throws<InvalidPluginExecutionException>(
                () => new PreventDuplicateContactByEmail().Execute(mock.ServiceProvider.Object));
        }

        /// <summary>
        /// Nested plugin call (depth 2) should skip duplicate validation entirely.
        /// Simulates the scenario where CreateChildContactOnAccountCreate triggers a contact create
        /// that would otherwise be blocked by the duplicate check.
        /// </summary>
        [Fact]
        public void NestedCall_SkipsDuplicateValidation()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "dup@example.com";

            var mock = PluginMockFactory.Create(target, "Create", 10, depth: 2);

            new PreventDuplicateContactByEmail().Execute(mock.ServiceProvider.Object);

            mock.OrganizationService.Verify(
                o => o.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Never);
        }

        /// <summary>
        /// Depth 1 (user-initiated) with duplicate email should still throw.
        /// Ensures the depth check does not accidentally disable validation for direct user creates.
        /// </summary>
        [Fact]
        public void DepthOne_DuplicateEmail_StillThrows()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "dup@example.com";

            var mock = PluginMockFactory.Create(target, "Create", 10, depth: 1);
            mock.OrganizationService
                .Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection(new[] { new Entity("contact") }));

            Assert.Throws<InvalidPluginExecutionException>(
                () => new PreventDuplicateContactByEmail().Execute(mock.ServiceProvider.Object));
        }
    }
}
