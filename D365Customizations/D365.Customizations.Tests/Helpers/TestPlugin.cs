/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Minimal PluginBase test double that increments CallCount when ExecuteBusinessLogic runs. Refer to following steps
**     1. Override ExecuteBusinessLogic to increment call counter for assertions
*/

using D365.Plugins.Common.Base;

namespace D365.Customizations.Tests.Helpers
{
    /// <summary>
    /// Test double that increments <see cref="CallCount"/> whenever business logic runs.
    /// </summary>
    public class TestPlugin : PluginBase
    {
        /// <summary>Number of times <see cref="ExecuteBusinessLogic"/> was entered.</summary>
        public int CallCount { get; private set; }

        /// <inheritdoc />
        protected override void ExecuteBusinessLogic(LocalPluginContext localContext)
        {
            CallCount++;
        }
    }
}
