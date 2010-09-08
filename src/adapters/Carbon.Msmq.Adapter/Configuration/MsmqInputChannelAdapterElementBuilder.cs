using Carbon.Core.Adapter.Factory;
using Carbon.Core.Configuration;
using Castle.Core.Configuration;

namespace Carbon.Msmq.Adapter.Configuration
{
    public class MsmqInputChannelAdapterElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "msmq-input-adapter";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var inputchannel = configuration.Attributes["input-channel"];
            var outputchannel = configuration.Attributes["output-channel"];
            var uri = configuration.Attributes["uri"];

            var adapter = Kernel.Resolve<IAdapterFactory>().BuildInputAdapterFromUri(inputchannel, uri);

            adapter.SetChannel(inputchannel);
            adapter.Uri = uri;

            ConfigureInputChannelAdapterBasicStrategies(configuration, adapter);
            RegisterSourceChannelAdapter(adapter);
            RegisterChannels(inputchannel, outputchannel);
        }
    }
}