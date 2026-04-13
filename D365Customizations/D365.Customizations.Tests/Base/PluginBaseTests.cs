using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace D365.Customizations.Tests.Base
{
    public class PluginBaseTests
    {
        [Fact]
        public void TC_B01_ValidServiceProvider_CallsExecuteBusinessLogic()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            var m = PluginMockFactory.CreatePluginMockContext(target, "Create", 10);
            var plugin = new TestPlugin();
            plugin.Execute(m.ServiceProvider.Object);
            Assert.Equal(1, plugin.BusinessLogicCallCount);
        }

        [Fact]
        public void TC_B02_NullTarget_DoesNotCallExecuteBusinessLogic()
        {
            var m = PluginMockFactory.CreatePluginMockContext(null, "Create", 10);
            var plugin = new TestPlugin();
            plugin.Execute(m.ServiceProvider.Object);
            Assert.Equal(0, plugin.BusinessLogicCallCount);
        }

        [Fact]
        public void TC_B03_MissingTargetKey_DoesNotCallExecuteBusinessLogic()
        {
            var m = PluginMockFactory.CreatePluginMockContext(null, "Create", 10);
            var plugin = new TestPlugin();
            plugin.Execute(m.ServiceProvider.Object);
            Assert.Equal(0, plugin.BusinessLogicCallCount);
        }
    }
}
