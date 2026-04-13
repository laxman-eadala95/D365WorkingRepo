using System;
using CustomPlugins;
using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Account
{
    public class CreateChildContactOnAccountCreateTests
    {
        [Fact]
        public void TC_A01_AccountCreate_CallsContactCreate()
        {
            var accountId = Guid.NewGuid();
            var target = new Entity(AccountConstants.EntityLogicalName) { Id = accountId };
            target[AccountConstants.AttributeName] = "Fabrikam";

            var m = PluginMockFactory.CreatePluginMockContext(target, "Create", 40, accountId);
            m.OrganizationService.Setup(o => o.Create(It.IsAny<Entity>())).Returns(Guid.NewGuid());

            var p = new CreateChildContactOnAccountCreate();
            p.Execute(m.ServiceProvider.Object);

            m.OrganizationService.Verify(o => o.Create(It.Is<Entity>(e => e.LogicalName == ContactConstants.EntityLogicalName)), Times.Once);
        }
    }
}
