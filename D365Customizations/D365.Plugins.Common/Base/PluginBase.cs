using System;
using Microsoft.Xrm.Sdk;

namespace D365.Plugins.Common.Base
{
    /// <summary>
    /// Template Method: extracts CRM services from the pipeline and delegates to
    /// <see cref="ExecuteBusinessLogic"/>.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            if (context == null || tracing == null || factory == null)
            {
                return;
            }

            if (!context.InputParameters.Contains("Target") || context.InputParameters["Target"] == null)
            {
                return;
            }

            if (!(context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            var service = factory.CreateOrganizationService(context.UserId);
            var local = new LocalPluginContext(context, service, tracing, target);
            ExecuteBusinessLogic(local);
        }

        /// <summary>
        /// Override with entity-specific behavior; pipeline plumbing is already resolved.
        /// </summary>
        protected abstract void ExecuteBusinessLogic(LocalPluginContext localContext);
    }
}
