using System;
using CustomPlugins.Services;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Account
{
    public class ChildContactServiceTests
    {
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
