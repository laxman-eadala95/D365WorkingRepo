using D365.Plugins.Common.Base;

namespace D365.Customizations.Tests.Helpers
{
    /// <summary>
    /// Test-only plugin to assert <see cref="PluginBase"/> pipeline behavior.
    /// </summary>
    public sealed class TestPlugin : PluginBase
    {
        public int BusinessLogicCallCount { get; private set; }

        /// <inheritdoc />
        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            BusinessLogicCallCount++;
        }
    }
}
