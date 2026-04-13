/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Tests CreateChildContactOnAccountCreate plugin with mocked OrganizationService and PluginMockFactory. Refer to following steps
**     1. Simulate Account Create with name and verify Create called once for contact entity
*/

using System;
using CustomPlugins;
using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Account
{
    /// <summary>
    /// Tests for <see cref="CreateChildContactOnAccountCreate"/> registration behavior against mocks.
    /// </summary>
    public class CreateChildContactOnAccountCreateTests
    {
        /// <summary>Plugin should call Create once for entity logical name contact when account is created.</summary>
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
