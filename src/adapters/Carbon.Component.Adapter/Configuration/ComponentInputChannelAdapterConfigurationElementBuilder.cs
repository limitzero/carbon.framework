using Carbon.Core.Adapter.Factory;
using Carbon.Core.Configuration;
using Castle.Core.Configuration;

namespace Carbon.Component.Adapter.Configuration
{
    public class ComponentInputChannelAdapterConfigurationElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "component-input-adapter";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var reference = configuration.Attributes["ref"];
            var inputchannel = configuration.Attributes["input-channel"];
            var method = configuration.Attributes["method"];
            var uri = configuration.Attributes["uri"];

            var adapter = Kernel.Resolve<IAdapterFactory>().BuildInputAdapterFromUri(inputchannel, uri);

            adapter.SetChannel(inputchannel);
            adapter.Uri = uri;

            ConfigureInputChannelAdapterBasicStrategies(configuration, adapter);
            RegisterSourceChannelAdapter(adapter);
            RegisterChannels(inputchannel, string.Empty);
        }
    }
}