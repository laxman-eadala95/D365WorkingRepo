/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Bundles pipeline services and Target entity so plugin subclasses avoid repeating service extraction and casting. Refer to following steps
**     1. Expose IPluginExecutionContext for stage, message, and parameters
**     2. Expose IOrganizationService scoped to the initiating user
**     3. Expose ITracingService and the pipeline Target Entity for business logic
*/

using Microsoft.Xrm.Sdk;

namespace D365.Plugins.Common.Base
{
    /// <summary>
    /// Carries the execution context, impersonated organization service, tracing service,
    /// and the current pipeline Target entity in one place.
    /// </summary>
    public class LocalPluginContext
    {
        /// <summary>Current plugin execution context (stage, message, InputParameters, etc.).</summary>
        public IPluginExecutionContext Context { get; }

        /// <summary>Organization service running as the initiating user (IPluginExecutionContext.UserId).</summary>
        public IOrganizationService Service { get; }

        /// <summary>Tracing service for diagnostic output visible in plugin trace logs.</summary>
        public ITracingService TracingService { get; }

        /// <summary>Primary entity in the pipeline (Create/Update/Delete Target).</summary>
        public Entity Target { get; }

        /// <summary>
        /// Creates a new context bundle for use inside PluginBase.ExecuteBusinessLogic.
        /// </summary>
        /// <param name="context">Plugin execution context from the platform.</param>
        /// <param name="service">Organization service created for the current user.</param>
        /// <param name="tracingService">Tracing service for logging.</param>
        /// <param name="target">The Target entity from InputParameters.</param>
        public LocalPluginContext(
            IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracingService,
            Entity target)
        {
            Context = context;
            Service = service;
            TracingService = tracingService;
            Target = target;
        }
    }
}
