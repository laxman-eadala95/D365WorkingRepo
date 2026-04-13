using System;
using CustomPlugins;
using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Account
{
    public class CreateChildContactOnAccountCreateTests
    {
        [Fact]
        public void AccountCreate_CreatesChildContact()
        {
            var accountId = Guid.NewGuid();
            var target = new Entity(AccountConstants.EntityLogicalName) { Id = accountId };
            target[AccountConstants.AttributeName] = "Fabrikam";

            var mock = PluginMockFactory.Create(target, "Create", 40, accountId);
            mock.OrganizationService
                .Setup(o => o.Create(It.IsAny<Entity>()))
                .Returns(Guid.NewGuid());

            new CreateChildContactOnAccountCreate().Execute(mock.ServiceProvider.Object);

            mock.OrganizationService.Verify(
                o => o.Create(It.Is<Entity>(e => e.LogicalName == "contact")),
                Times.Once);
        }
    }
}
