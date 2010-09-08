using Carbon.Core.Adapter.Factory;
using Carbon.Core.Configuration;
using Castle.Core.Configuration;

namespace Carbon.Sql.Adapter.Configuration
{
    public class SqlOutputChannelAdapterConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "sql-output-adapter";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var inputchannel = configuration.Attributes["input-channel"];
            var outputchannel = configuration.Attributes["output-channel"];
            var uri = configuration.Attributes["uri"];

            var adapter = Kernel.Resolve<IAdapterFactory>().BuildOutputAdapterFromUri<SqlOutputChannelAdapter>(inputchannel, uri);

            adapter.SetChannel(inputchannel);
            adapter.Uri = uri;

            base.RegisterTargetChannelAdapter(adapter);
            base.RegisterChannels(inputchannel, outputchannel);
        }


    }
}