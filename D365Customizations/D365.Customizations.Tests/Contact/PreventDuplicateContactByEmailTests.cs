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
        public void TC_P01_UniqueEmail_NoException()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "new@x.com";

            var m = PluginMockFactory.CreatePluginMockContext(target, "Create", 10);
            m.OrganizationService.Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection());

            var p = new PreventDuplicateContactByEmail();
            p.Execute(m.ServiceProvider.Object);
        }

        [Fact]
        public void TC_P02_DuplicateEmail_InvalidPluginExecutionException()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            target[ContactConstants.AttributeEmailAddress1] = "dup@x.com";

            var m = PluginMockFactory.CreatePluginMockContext(target, "Create", 10);
            m.OrganizationService.Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new[] { new Entity(ContactConstants.EntityLogicalName) }));

            var p = new PreventDuplicateContactByEmail();
            Assert.Throws<InvalidPluginExecutionException>(() => p.Execute(m.ServiceProvider.Object));
        }
    }
}
