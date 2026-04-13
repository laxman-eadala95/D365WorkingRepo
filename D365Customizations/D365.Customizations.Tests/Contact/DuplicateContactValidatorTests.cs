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
        public void EmailExists_ReturnsTrue_WhenMatchFound()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection(new[] { new Entity("contact") }));

            var validator = new DuplicateContactValidator(org.Object);

            Assert.True(validator.EmailExists("test@example.com"));
        }

        [Fact]
        public void EmailExists_ReturnsFalse_WhenNoMatch()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection());

            var validator = new DuplicateContactValidator(org.Object);

            Assert.False(validator.EmailExists("new@example.com"));
        }

        [Fact]
        public void Validate_ThrowsWithExactMessage_WhenDuplicate()
        {
            var org = new Mock<IOrganizationService>();
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection(new[] { new Entity("contact") }));

            var validator = new DuplicateContactValidator(org.Object);

            var ex = Assert.Throws<InvalidPluginExecutionException>(
                () => validator.ValidateNoDuplicateEmail("dup@example.com"));

            Assert.Equal(ContactConstants.DuplicateEmailMessage, ex.Message);
        }

        [Fact]
        public void Validate_SkipsCheck_WhenEmailIsNull()
        {
            var org = new Mock<IOrganizationService>();
            var validator = new DuplicateContactValidator(org.Object);

            validator.ValidateNoDuplicateEmail(null);

            org.Verify(o => o.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Never);
        }

        [Fact]
        public void Query_UsesTopCountOne_ForPerformance()
        {
            QueryExpression captured = null;
            var org = new Mock<IOrganizationService>();
            // SDK exposes RetrieveMultiple(QueryBase); Moq callbacks must use the same parameter type.
            org.Setup(o => o.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Callback<QueryBase>(q => captured = q as QueryExpression)
                .Returns(new EntityCollection());

            var validator = new DuplicateContactValidator(org.Object);
            validator.EmailExists("t@t.com");

            Assert.NotNull(captured);
            Assert.Equal(1, captured.TopCount);
        }
    }
}
