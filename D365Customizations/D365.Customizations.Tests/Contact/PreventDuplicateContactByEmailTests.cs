using CustomPlugins;
using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Contact
{
    public class PreventDuplicateContactByEmailTests
    {
        [Fact]
        public void UniqueEmail_NoException()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "unique@example.com";

            var mock = PluginMockFactory.Create(target, "Create", 10);
            mock.OrganizationService
                .Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection());

            new PreventDuplicateContactByEmail().Execute(mock.ServiceProvider.Object);
        }

        [Fact]
        public void DuplicateEmail_ThrowsInvalidPluginExecutionException()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "dup@example.com";

            var mock = PluginMockFactory.Create(target, "Create", 10);
            mock.OrganizationService
                .Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new[] { new Entity("contact") }));

            Assert.Throws<InvalidPluginExecutionException>(
                () => new PreventDuplicateContactByEmail().Execute(mock.ServiceProvider.Object));
        }
    }
}
