using System;
using Microsoft.Xrm.Sdk;

namespace D365.Plugins.Common.Base
{
    /// <summary>
    /// Base class for all D365 plugins. Handles the repetitive service extraction
    /// from the pipeline so each plugin only implements its own business logic.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Guard: if Target is missing or not an Entity, nothing to do
            if (!context.InputParameters.Contains("Target"))
                return;

            if (!(context.InputParameters["Target"] is Entity target))
                return;

            var service = factory.CreateOrganizationService(context.UserId);
            var localContext = new LocalPluginContext(context, service, tracing, target);

            ExecuteBusinessLogic(localContext);
        }

        /// <summary>
        /// Override this with your entity-specific logic. The pipeline plumbing is already done.
        /// </summary>
        protected abstract void ExecuteBusinessLogic(LocalPluginContext localContext);
    }
}
