/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Factory for Moq IServiceProvider chains matching the Dataverse plugin host. Refer to following steps
**     1. Wire InputParameters Target, message, stage, PrimaryEntityId, and org service factory
**     2. Return PluginMockContext with mocks for Verify and further Setup in tests
*/

using System;
using Microsoft.Xrm.Sdk;
using Moq;

namespace D365.Customizations.Tests.Helpers
{
    /// <summary>
    /// Factory for mock plugin dependencies: execution context with InputParameters, org service factory, tracing.
    /// </summary>
    public static class PluginMockFactory
    {
        /// <summary>
        /// Exposes the mocks returned by <see cref="Create"/> for further Setup/Verify in tests.
        /// </summary>
        public class PluginMockContext
        {
            /// <summary>Root service provider passed to <c>IPlugin.Execute</c>.</summary>
            public Mock<IServiceProvider> ServiceProvider { get; set; }

            /// <summary>Organization service from the mocked factory.</summary>
            public Mock<IOrganizationService> OrganizationService { get; set; }

            /// <summary>Tracing service mock.</summary>
            public Mock<ITracingService> TracingService { get; set; }

            /// <summary>Plugin execution context mock.</summary>
            public Mock<IPluginExecutionContext> PluginContext { get; set; }
        }

        /// <summary>
        /// Creates a fully wired mock context. Optionally sets Target in InputParameters and PrimaryEntityId.
        /// </summary>
        /// <param name="target">Entity for InputParameters["Target"], or null to omit Target.</param>
        /// <param name="message">Pipeline message name (e.g. Create).</param>
        /// <param name="stage">Pipeline stage (e.g. 10 Pre-validation, 40 Post-operation).</param>
        /// <param name="primaryEntityId">Overrides primary entity id; defaults to target.Id or a new guid.</param>
        public static PluginMockContext Create(Entity target, string message, int stage, Guid? primaryEntityId = null)
        {
            var orgService = new Mock<IOrganizationService>(MockBehavior.Strict);
            var tracing = new Mock<ITracingService>(MockBehavior.Loose);
            var context = new Mock<IPluginExecutionContext>(MockBehavior.Strict);

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
