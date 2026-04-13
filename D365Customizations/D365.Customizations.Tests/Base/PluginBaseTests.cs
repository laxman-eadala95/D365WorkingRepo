/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Unit tests for PluginBase ensuring business logic runs only when Target is a valid Entity. Refer to following steps
**     1. ValidTarget_CallsBusinessLogic asserts TestPlugin runs when Target is present
**     2. NullTarget_SkipsBusinessLogic asserts no call when Target is omitted
*/

using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace D365.Customizations.Tests.Base
{
    /// <summary>
    /// Unit tests for <see cref="D365.Plugins.Common.Base.PluginBase"/> guard behavior and delegation.
    /// </summary>
    public class PluginBaseTests
    {
        /// <summary>Valid Target in InputParameters should invoke ExecuteBusinessLogic once.</summary>
        [Fact]
        public void ValidTarget_CallsBusinessLogic()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            var mock = PluginMockFactory.Create(target, "Create", 10);

            var plugin = new TestPlugin();
            plugin.Execute(mock.ServiceProvider.Object);

            Assert.Equal(1, plugin.CallCount);
        }

        /// <summary>Missing Target should skip business logic entirely.</summary>
        [Fact]
        public void NullTarget_SkipsBusinessLogic()
        {
            var mock = PluginMockFactory.Create(null, "Create", 10);

            var plugin = new TestPlugin();
            plugin.Execute(mock.ServiceProvider.Object);

            Assert.Equal(0, plugin.CallCount);
        }
    }
}
