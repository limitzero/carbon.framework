using Carbon.Core.Adapter.Factory;
using Carbon.Core.Configuration;
using Carbon.File.Adapter.Strategies;
using Castle.Core.Configuration;

namespace Carbon.File.Adapter.Configuration
{
    public class FileOutputChannelAdapterConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "file-output-adapter";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var inputchannel = configuration.Attributes["input-channel"];
            var outputchannel = configuration.Attributes["output-channel"];
            var uri = configuration.Attributes["uri"];

            var adapter = Kernel.Resolve<IAdapterFactory>().BuildOutputAdapterFromUri<FileOutputAdapter>(inputchannel, uri);

            adapter.SetChannel(inputchannel);
            adapter.Uri = uri;

            // grab the strategies:
            var strategies = configuration.Children["strategies"];

            if (strategies != null)
            {
                this.CreateFileNamingStrategy(strategies, adapter);
            }

            base.RegisterTargetChannelAdapter(adapter);
            base.RegisterChannels(inputchannel, outputchannel);
        }

        /// <summary>
        /// This will register the strategy for naming files on the target location upon delivery.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="adapter"></param>
        private void CreateFileNamingStrategy(IConfiguration configuration, FileOutputAdapter adapter)
        {
            var filenameStrategy = configuration.Children["filename"];
            if (filenameStrategy != null)
            {
                var fileName = filenameStrategy.Attributes["name"];
                var fileExtension = filenameStrategy.Attributes["extension"];

                if (!string.IsNullOrEmpty(fileName) & !string.IsNullOrEmpty(fileExtension))
                {
                    adapter.FileNameStrategy = new DefaultFileNameStrategy(fileName, fileExtension);
                }
            }
        }
    }
}