/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Base plugin entry for Dataverse that resolves pipeline services and invokes entity-specific logic only when a valid Target exists. Refer to following steps
**     1. Obtain IPluginExecutionContext, ITracingService, and IOrganizationServiceFactory from IServiceProvider
**     2. Validate InputParameters contains an Entity Target; exit early otherwise
**     3. Create impersonated IOrganizationService and LocalPluginContext, then call ExecuteBusinessLogic
*/

using System;
using Microsoft.Xrm.Sdk;

namespace D365.Plugins.Common.Base
{
    /// <summary>
    /// Base class for all D365 plugins. Centralizes retrieval of execution context,
    /// tracing, organization service factory, and the Target entity so derived plugins
    /// only implement business rules.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        /// <summary>
        /// Maximum allowed pipeline depth. Override in a derived plugin if a deeper chain is expected.
        /// D365 enforces a hard limit of 8; this default provides an earlier, clearer exit.
        /// </summary>
        protected virtual int MaxDepth => 4;

        /// <summary>
        /// Plugin pipeline entry. Extracts services from IServiceProvider,
        /// validates depth and InputParameters["Target"], and calls ExecuteBusinessLogic.
        /// </summary>
        /// <param name="serviceProvider">Runtime service provider supplied by the platform.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            if (context.Depth > MaxDepth)
            {
                tracing.Trace("PluginBase: Depth {0} exceeds max {1}. Exiting.", context.Depth, MaxDepth);
                return;
            }

            // No Target or wrong type: nothing to process (avoids null reference downstream).
            if (!context.InputParameters.Contains("Target"))
                return;

            if (!(context.InputParameters["Target"] is Entity target))
                return;

            var service = factory.CreateOrganizationService(context.UserId);
            var localContext = new LocalPluginContext(context, service, tracing, target);

            ExecuteBusinessLogic(localContext);
        }

        /// <summary>
        /// Implement entity-specific behavior. Called only when Target is a non-null Entity.
        /// </summary>
        /// <param name="localContext">Bundled context, service, tracing, and Target.</param>
        protected abstract void ExecuteBusinessLogic(LocalPluginContext localContext);
    }
}
