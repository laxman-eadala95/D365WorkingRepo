using D365.Customizations.Tests.Helpers;
using D365.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace D365.Customizations.Tests.Base
{
    public class PluginBaseTests
    {
        [Fact]
        public void ValidTarget_CallsBusinessLogic()
        {
            var target = new Entity(ContactConstants.EntityLogicalName);
            var mock = PluginMockFactory.Create(target, "Create", 10);

            var plugin = new TestPlugin();
            plugin.Execute(mock.ServiceProvider.Object);

            Assert.Equal(1, plugin.CallCount);
        }

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
