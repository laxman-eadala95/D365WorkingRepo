using CustomPlugins.Services;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace D365.Customizations.Tests.Contact
{
    public class DuplicateContactValidatorTests
    {
        [Fact]
        public void TC_PV01_EmailExists_ReturnsTrue_WhenContactFound()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new[] { new Entity(ContactConstants.EntityLogicalName) }));
            var v = new DuplicateContactValidator(org.Object);
            Assert.True(v.EmailExists("a@b.com"));
        }

        [Fact]
        public void TC_PV02_EmailExists_ReturnsFalse_WhenNone()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection());
            var v = new DuplicateContactValidator(org.Object);
            Assert.False(v.EmailExists("a@b.com"));
        }

        [Fact]
        public void TC_PV04_ValidateDuplicate_ThrowsWithExactMessage()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new[] { new Entity(ContactConstants.EntityLogicalName) }));
            var v = new DuplicateContactValidator(org.Object);
            var ex = Assert.Throws<InvalidPluginExecutionException>(() => v.ValidateNoDuplicateEmail("x@y.com"));
            Assert.Equal(ContactConstants.DuplicateEmailMessage, ex.Message);
        }

        [Fact]
        public void TC_PV05_NullEmail_DoesNotQuery()
        {
            var org = new Mock<IOrganizationService>();
            var v = new DuplicateContactValidator(org.Object);
            v.ValidateNoDuplicateEmail(null);
            org.Verify(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()), Times.Never);
        }

        [Fact]
        public void TC_PV08_QueryUsesTopCountOne()
        {
            QueryExpression captured = null;
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Callback<QueryExpression>(q => captured = q)
                .Returns(new EntityCollection());
            var v = new DuplicateContactValidator(org.Object);
            v.EmailExists("t@t.com");
            Assert.NotNull(captured);
            Assert.Equal(1, captured.TopCount);
        }
    }
}
