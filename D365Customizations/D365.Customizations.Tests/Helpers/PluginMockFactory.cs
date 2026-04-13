using System;
using Microsoft.Xrm.Sdk;
using Moq;

namespace D365.Customizations.Tests.Helpers
{
    /// <summary>
    /// Builds the mock IServiceProvider chain that D365 normally provides to plugins.
    /// </summary>
    public static class PluginMockFactory
    {
        public class PluginMockContext
        {
            public Mock<IServiceProvider> ServiceProvider { get; set; }
            public Mock<IOrganizationService> OrganizationService { get; set; }
            public Mock<ITracingService> TracingService { get; set; }
            public Mock<IPluginExecutionContext> PluginContext { get; set; }
        }

        public static PluginMockContext Create(Entity target, string message, int stage, Guid? primaryEntityId = null)
        {
            var orgService = new Mock<IOrganizationService>(MockBehavior.Strict);
            var tracing = new Mock<ITracingService>(MockBehavior.Loose);
            var context = new Mock<IPluginExecutionContext>(MockBehavior.Strict);

            // Wire up InputParameters with optional Target
            var inputParams = new ParameterCollection();
            if (target != null)
                inputParams["Target"] = target;

            var entityId = primaryEntityId
                ?? (target != null && target.Id != Guid.Empty ? target.Id : Guid.NewGuid());

            context.SetupGet(c => c.MessageName).Returns(message);
            context.SetupGet(c => c.Stage).Returns(stage);
            context.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
            context.SetupGet(c => c.PrimaryEntityId).Returns(entityId);
            context.SetupGet(c => c.InputParameters).Returns(inputParams);
            context.SetupGet(c => c.OutputParameters).Returns(new ParameterCollection());

            var factory = new Mock<IOrganizationServiceFactory>(MockBehavior.Strict);
            factory.Setup(f => f.CreateOrganizationService(It.IsAny<Guid?>())).Returns(orgService.Object);

            var provider = new Mock<IServiceProvider>(MockBehavior.Strict);
            provider.Setup(p => p.GetService(typeof(IPluginExecutionContext))).Returns(context.Object);
            provider.Setup(p => p.GetService(typeof(ITracingService))).Returns(tracing.Object);
            provider.Setup(p => p.GetService(typeof(IOrganizationServiceFactory))).Returns(factory.Object);

            return new PluginMockContext
            {
                ServiceProvider = provider,
                OrganizationService = orgService,
                TracingService = tracing,
                PluginContext = context
            };
        }
    }
}
