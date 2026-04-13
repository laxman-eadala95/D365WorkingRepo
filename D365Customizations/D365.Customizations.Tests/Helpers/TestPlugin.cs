using D365.Plugins.Common.Base;

namespace D365.Customizations.Tests.Helpers
{
    /// <summary>
    /// Minimal plugin for testing PluginBase pipeline behavior.
    /// </summary>
    public class TestPlugin : PluginBase
    {
        public int CallCount { get; private set; }

        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            CallCount++;
        }
    }
}
