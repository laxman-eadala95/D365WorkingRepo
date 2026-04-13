using Microsoft.Xrm.Sdk;

namespace D365.Plugins.Common.Base
{
    /// <summary>
    /// Bundles services and the pipeline target for plugin business logic.
    /// </summary>
    public sealed class LocalPluginContext
    {
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

        public IPluginExecutionContext Context { get; }

        public IOrganizationService Service { get; }

        public ITracingService TracingService { get; }

        public Entity Target { get; }
    }
}
