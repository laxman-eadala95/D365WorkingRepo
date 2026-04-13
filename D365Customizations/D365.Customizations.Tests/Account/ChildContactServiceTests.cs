/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Unit tests for ChildContactService entity fields and parent account EntityReference. Refer to following steps
**     1. Verify Create invocation, firstname/lastname, and parentcustomerid to account
*/

using System;
using CustomPlugins.Services;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Account
{
    /// <summary>
    /// Tests for <see cref="ChildContactService"/> Create behavior and field population.
    /// </summary>
    public class ChildContactServiceTests
    {
        /// <summary>Service should invoke IOrganizationService.Create exactly once.</summary>
        [Fact]
        public void CreateChildContact_CallsServiceCreate()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.Create(It.IsAny<Entity>())).Returns(Guid.NewGuid());
            var trace = new Mock<ITracingService>();

            var svc = new ChildContactService(org.Object, trace.Object);
            svc.CreateChildContact(Guid.NewGuid(), "Acme");

            org.Verify(o => o.Create(It.IsAny<Entity>()), Times.Once);
        }

        /// <summary>Created entity should use default first name and account name as last name.</summary>
        [Fact]
        public void CreatedContact_HasCorrectFields()
        {
            Entity captured = null;
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.Create(It.IsAny<Entity>()))
                .Callback<Entity>(e => captured = e)
                .Returns(Guid.NewGuid());
            var trace = new Mock<ITracingService>();

            new ChildContactService(org.Object, trace.Object)
                .CreateChildContact(Guid.NewGuid(), "Acme");

            Assert.NotNull(captured);
            Assert.Equal("Default", captured.GetAttributeValue<string>("firstname"));
            Assert.Equal("Acme", captured.GetAttributeValue<string>("lastname"));
        }

        /// <summary>Parent customer lookup should reference the account id and logical name.</summary>
        [Fact]
        public void CreatedContact_HasCorrectParentReference()
        {
            Entity captured = null;
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.Create(It.IsAny<Entity>()))
                .Callback<Entity>(e => captured = e)
                .Returns(Guid.NewGuid());
            var trace = new Mock<ITracingService>();

            var accountId = Guid.NewGuid();
            new ChildContactService(org.Object, trace.Object)
                .CreateChildContact(accountId, "X");

            Assert.NotNull(captured);
            var parent = captured.GetAttributeValue<EntityReference>("parentcustomerid");
            Assert.Equal("account", parent.LogicalName);
            Assert.Equal(accountId, parent.Id);
        }
    }
}
