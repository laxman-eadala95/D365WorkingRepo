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
        public void TC_AV01_CreateCalledOnce()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.Create(It.IsAny<Entity>())).Returns(Guid.NewGuid());
            var trace = new Mock<ITracingService>();
            var s = new ChildContactService(org.Object, trace.Object);
            s.CreateChildContact(Guid.NewGuid(), "Acme");
            org.Verify(o => o.Create(It.IsAny<Entity>()), Times.Once);
        }

        [Fact]
        public void TC_AV02_FieldsSetCorrectly()
        {
            Entity captured = null;
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.Create(It.IsAny<Entity>()))
                .Callback<Entity>(e => captured = e)
                .Returns(Guid.NewGuid());
            var trace = new Mock<ITracingService>();
            var s = new ChildContactService(org.Object, trace.Object);
            var aid = Guid.NewGuid();
            s.CreateChildContact(aid, "Acme");

            Assert.NotNull(captured);
            Assert.Equal(ContactConstants.DefaultFirstName, captured.GetAttributeValue<string>(ContactConstants.AttributeFirstName));
            Assert.Equal("Acme", captured.GetAttributeValue<string>(ContactConstants.AttributeLastName));
        }

        [Fact]
        public void TC_AV03_ParentReference_IsAccount()
        {
            Entity captured = null;
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.Create(It.IsAny<Entity>()))
                .Callback<Entity>(e => captured = e)
                .Returns(Guid.NewGuid());
            var trace = new Mock<ITracingService>();
            var s = new ChildContactService(org.Object, trace.Object);
            var aid = Guid.NewGuid();
            s.CreateChildContact(aid, "X");
            Assert.NotNull(captured);
            var parent = captured.GetAttributeValue<EntityReference>(ContactConstants.AttributeParentCustomerId);
            Assert.Equal(AccountConstants.EntityLogicalName, parent.LogicalName);
            Assert.Equal(aid, parent.Id);
        }
    }
}
