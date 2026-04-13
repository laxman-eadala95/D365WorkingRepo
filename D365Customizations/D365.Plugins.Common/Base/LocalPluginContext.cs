using Microsoft.Xrm.Sdk;

namespace D365.Plugins.Common.Base
{
    /// <summary>
    /// Bundles the pipeline services and target entity into a single object
    /// so plugin subclasses don't have to repeat the extraction boilerplate.
    /// </summary>
    public class LocalPluginContext
    {
        public IPluginExecutionContext Context { get; }
        public IOrganizationService Service { get; }
        public ITracingService TracingService { get; }
        public Entity Target { get; }

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
