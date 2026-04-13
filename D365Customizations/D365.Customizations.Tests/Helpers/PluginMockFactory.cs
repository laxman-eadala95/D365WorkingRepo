using System;
using Microsoft.Xrm.Sdk;
using Moq;

namespace D365.Customizations.Tests.Helpers
{
    /// <summary>
    /// Builds a mock <see cref="IServiceProvider"/> chain for plugin pipeline tests.
    /// </summary>
    public static class PluginMockFactory
    {
        public sealed class PluginMockContext
        {
            public Mock<IServiceProvider> ServiceProvider { get; set; }

            public Mock<IOrganizationService> OrganizationService { get; set; }

            public Mock<ITracingService> TracingService { get; set; }

            public Mock<IPluginExecutionContext> PluginContext { get; set; }
        }

        public static PluginMockContext CreatePluginMockContext(
            Entity target,
            string messageName,
            int stage,
            Guid? primaryEntityId = null)
        {
            var org = new Mock<IOrganizationService>(MockBehavior.Strict);
            var trace = new Mock<ITracingService>(MockBehavior.Loose);
            var ctx = new Mock<IPluginExecutionContext>(MockBehavior.Strict);

            var input = new ParameterCollection();
            if (target != null)
            {
                input["Target"] = target;
            }

            var peid = primaryEntityId ?? (target != null && target.Id != Guid.Empty ? target.Id : Guid.NewGuid());
            ctx.SetupGet(c => c.MessageName).Returns(messageName);
            ctx.SetupGet(c => c.Stage).Returns(stage);
            ctx.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
            ctx.SetupGet(c => c.PrimaryEntityId).Returns(peid);
            ctx.SetupGet(c => c.InputParameters).Returns(input);
            ctx.SetupGet(c => c.OutputParameters).Returns(new ParameterCollection());

            var factory = new Mock<IOrganizationServiceFactory>(MockBehavior.Strict);
            factory.Setup(f => f.CreateOrganizationService(It.IsAny<Guid?>())).Returns(org.Object);

            var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProvider.Setup(p => p.GetService(typeof(IPluginExecutionContext))).Returns(ctx.Object);
            serviceProvider.Setup(p => p.GetService(typeof(ITracingService))).Returns(trace.Object);
            serviceProvider.Setup(p => p.GetService(typeof(IOrganizationServiceFactory))).Returns(factory.Object);

            return new PluginMockContext
            {
                ServiceProvider = serviceProvider,
                OrganizationService = org,
                TracingService = trace,
                PluginContext = ctx
            };
        }
    }
}
