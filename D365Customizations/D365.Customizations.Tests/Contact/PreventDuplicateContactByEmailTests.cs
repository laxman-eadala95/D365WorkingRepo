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
    /// Integration-style tests for <see cref="PreventDuplicateContactByEmail"/> against <see cref="PluginMockFactory"/>.
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
    }
}
