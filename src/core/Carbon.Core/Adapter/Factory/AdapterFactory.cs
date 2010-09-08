using Carbon.Channel.Registry;
using Carbon.Core.Adapter.Impl.Null;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;

namespace Carbon.Core.Adapter.Factory
{
    /// <summary>
    /// Concrete instance of a factory for creating adapters based on the uri configuration.
    /// </summary>
    public class AdapterFactory : IAdapterFactory
    {
        private readonly IObjectBuilder m_container;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="container"></param>
        public AdapterFactory(IObjectBuilder container)
        {
            m_container = container;
        }

        /// <summary>
        /// This will build a input channel adapter based on the uri configuration.
        /// </summary>
        /// <param name="uri">Uri for defining the adapter to retreive the message from a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractInputChannelAdapter"/>
        /// </returns>
        public AbstractInputChannelAdapter BuildInputAdapterFromUri(string uri)
        {
            return this.BuildInputAdapterFromUri(string.Empty, uri);
        }

        /// <summary>
        /// This will build a input channel adapter based on the uri configuration 
        /// and set the input channel name (if provided).
        /// </summary>
        /// <param name="channelName">Name of the input channel for passing the retreived message to.</param>
        /// <param name="uri">Uri for defining the adapter to retreive the message from a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractInputChannelAdapter"/>
        /// </returns>
        public AbstractInputChannelAdapter BuildInputAdapterFromUri(string channelName, string uri)
        {
            AbstractInputChannelAdapter inputChannelAdapter = new NullInputChannelAdapter();
            var registry = m_container.Resolve<IAdapterRegistry>();
            inputChannelAdapter = registry.FindInputAdapterFromUriScheme(uri);
            inputChannelAdapter.Uri = uri;

            // find the channel for the message to be loaded to from the uri location:
            if (!string.IsNullOrEmpty(channelName))
            {
                var channelRegistry = m_container.Resolve<IChannelRegistry>();
                var channel = channelRegistry.FindChannel(channelName);

                if (channel is NullChannel)
                    channelRegistry.RegisterChannel(channelName);

                inputChannelAdapter.SetChannel(channelName);
            }

            return inputChannelAdapter;
        }


        public TAdapter BuildInputAdapterFromUri<TAdapter>(string uri)
            where TAdapter : AbstractInputChannelAdapter
        {
            return this.BuildInputAdapterFromUri(string.Empty, uri) as TAdapter;
        }

        public TAdapter BuildInputAdapterFromUri<TAdapter>(string channelName, string uri)
                where TAdapter : AbstractInputChannelAdapter
        {
            return this.BuildInputAdapterFromUri(channelName, uri) as TAdapter;
        }


        /// <summary>
        /// This will build a target channel adapter based on the uri configuration.
        /// </summary>
        /// <param name="uri">Uri for defining the adapter for sending the channel message to a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractOutputChannelAdapter"/>
        /// </returns>
        public AbstractOutputChannelAdapter BuildOutputAdapterFromUri(string uri)
        {
            return this.BuildOutputAdapterFromUri(string.Empty, uri);
        }

        /// <summary>
        /// This will build a target channel adapter based on the uri configuration
        /// and set the channel name (if specified) for extracting the message.
        /// </summary>
        /// <param name="channelName">Name of the channel to inspect for extracting the message.</param>
        /// <param name="uri">Uri for defining the adapter for sending the channel message to a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractOutputChannelAdapter"/>
        /// </returns>
        public AbstractOutputChannelAdapter BuildOutputAdapterFromUri(string channelName, string uri)
        {
            AbstractOutputChannelAdapter outputChannelAdapter = new NullOutputChannelAdapter();

            var registry = m_container.Resolve<IAdapterRegistry>();
            outputChannelAdapter = registry.FindOutputAdapterFromUriScheme(uri);
            outputChannelAdapter.Uri = uri;

            // find the channel for the message to be loaded to from the uri location:
            if (!string.IsNullOrEmpty(channelName))
            {
                var channelRegistry = m_container.Resolve<IChannelRegistry>();
                var channel = channelRegistry.FindChannel(channelName);

                if (channel is NullChannel)
                    channelRegistry.RegisterChannel(channelName);

                outputChannelAdapter.SetChannel(channelName);
            }

            return outputChannelAdapter;
        }

        public TAdapter BuildOutputAdapterFromUri<TAdapter>(string uri)
             where TAdapter : AbstractOutputChannelAdapter
        {
            return this.BuildOutputAdapterFromUri(string.Empty, uri) as TAdapter;
        }

        public TAdapter BuildOutputAdapterFromUri<TAdapter>(string channelName, string uri)
                where TAdapter : AbstractOutputChannelAdapter
        {
            return this.BuildOutputAdapterFromUri(channelName, uri) as TAdapter;
        }
    }
}