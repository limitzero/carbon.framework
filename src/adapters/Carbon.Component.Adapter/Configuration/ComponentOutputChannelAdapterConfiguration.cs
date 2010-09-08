using Carbon.Core.Adapter.Factory;
using Carbon.Core.Configuration;
using Castle.Core.Configuration;

namespace Carbon.Component.Adapter.Configuration
{
    public class ComponentOutputChannelAdapterConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "component-output-adapter";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var reference = configuration.Attributes["ref"];
            var channel = configuration.Attributes["channel"];
            var method = configuration.Attributes["method"];
            var uri = configuration.Attributes["uri"];

            var adapter = Kernel.Resolve<IAdapterFactory>().BuildOutputAdapterFromUri<ComponentOutputAdapter>(channel, uri);

            adapter.SetChannel(channel);
            adapter.Uri = uri;

            base.RegisterTargetChannelAdapter(adapter);
            base.RegisterChannels(channel, string.Empty);
        }
    }
}