namespace Carbon.Core.Adapter.Factory
{
    /// <summary>
    /// Contract for bulding instances of adapters based on the uri scheme.
    /// </summary>
    public interface IAdapterFactory
    {
        /// <summary>
        /// This will build a input channel adapter based on the uri configuration 
        /// and set the input channel name (if provided).
        /// </summary>
        /// <param name="channelName">Name of the input channel for passing the retreived message to.</param>
        /// <param name="uri">Uri for defining the adapter to retreive the message from a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractInputChannelAdapter"/>
        /// </returns>
        AbstractInputChannelAdapter BuildInputAdapterFromUri(string channelName, string uri);

        /// <summary>
        /// This will build a input channel adapter based on the uri configuration 
        /// and set the input channel name (if provided).
        /// </summary>
        /// <param name="channelName">Name of the input channel for passing the retreived message to.</param>
        /// <param name="uri">Uri for defining the adapter to retreive the message from a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractInputChannelAdapter"/>
        /// </returns>
        TAdapter BuildInputAdapterFromUri<TAdapter>(string channelName, string uri)
                where TAdapter : AbstractInputChannelAdapter;

        /// <summary>
        /// This will build a target channel adapter based on the uri configuration 
        /// and set the input channel name (if provided).
        /// </summary>
        /// <param name="channelName">Name of the input channel for reading the channel message.</param>
        /// <param name="uri">Uri for defining the adapter for sending the channel message to a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractOutputChannelAdapter"/>
        /// </returns>
        AbstractOutputChannelAdapter BuildOutputAdapterFromUri(string channelName, string uri);

        /// <summary>
        /// This will build a target channel adapter based on the uri configuration 
        /// and set the input channel name (if provided).
        /// </summary>
        /// <param name="channelName">Name of the input channel for reading the channel message.</param>
        /// <param name="uri">Uri for defining the adapter for sending the channel message to a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractOutputChannelAdapter"/>
        /// </returns>
        TAdapter BuildOutputAdapterFromUri<TAdapter>(string channelName, string uri)
                where TAdapter : AbstractOutputChannelAdapter;

        /// <summary>
        /// This will build a input channel adapter based on the uri configuration.
        /// </summary>
        /// <param name="uri">Uri for defining the adapter to retreive the message from a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractInputChannelAdapter"/>
        /// </returns>
        AbstractInputChannelAdapter BuildInputAdapterFromUri(string uri);

        /// <summary>
        /// This will build a input channel adapter based on the uri configuration.
        /// </summary>
        /// <param name="uri">Uri for defining the adapter to retreive the message from a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractInputChannelAdapter"/>
        /// </returns>
        TAdapter BuildInputAdapterFromUri<TAdapter>(string uri)
            where TAdapter : AbstractInputChannelAdapter;

        /// <summary>
        /// This will build a target channel adapter based on the uri configuration.
        /// </summary>
        /// <param name="uri">Uri for defining the adapter for sending the channel message to a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractOutputChannelAdapter"/>
        /// </returns>
        AbstractOutputChannelAdapter BuildOutputAdapterFromUri(string uri);

        /// <summary>
        /// This will build a target channel adapter based on the uri configuration.
        /// </summary>
        /// <param name="uri">Uri for defining the adapter for sending the channel message to a particular location.</param>
        /// <returns>
        /// <seealso cref="AbstractOutputChannelAdapter"/>
        /// </returns>
        TAdapter BuildOutputAdapterFromUri<TAdapter>(string uri)
            where TAdapter : AbstractOutputChannelAdapter;

    }
}