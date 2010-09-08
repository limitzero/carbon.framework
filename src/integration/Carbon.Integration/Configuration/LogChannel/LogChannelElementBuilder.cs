using System;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Serialization;
using Castle.Core.Configuration;

namespace Carbon.Integration.Configuration.LogChannel
{
    public class LogChannelElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "log-channel";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var name = configuration.Attributes["name"];

            if(string.IsNullOrEmpty(name))
                return;

            if(name.Trim() == "*")
                foreach (var channel in Kernel.Resolve<IChannelRegistry>().Channels)
                {
                    this.ConfigureLoggingForChannel(channel.Name);
                }
            else
            {
                this.ConfigureLoggingForChannel(name.Trim());
            }
        }

        private void ConfigureLoggingForChannel(string channelName)
        {
            // by default, the channel is created as a queue channel that 
            // has a polling policy of one thread per request at a wait frequency of one 
            // second between requests, also it has a default scheduling policy of 
            // sending a request every second (the polling policy is the default):
            try
            {
                this.Kernel.Resolve<IChannelRegistry>().RegisterChannel(channelName.Trim());
            }
            catch (Exception exception)
            {
                // already built and registered....
            }

            var channel = this.Kernel.Resolve<IChannelRegistry>().FindChannel(channelName);

            if (!(channel is NullChannel))
            {
                channel.RegisterActionsOnMessageReceived(LogChannelMessageReceived);
                channel.RegisterActionsOnMessageSend(LogChannelMessageDelivered);
            }
        }

        private void LogChannelMessageReceived(ChannelMessageReceivedEventArgs eventArgs)
        {
            var template = this.Kernel.Resolve<IAdapterMessagingTemplate>();

            var payload = this.ResolveEnvelopePayload(eventArgs.Envelope);

            var msg = string.Format("Channel '{0}' message received. Contents: {1}", eventArgs.Channel,
                                    payload);

            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private void LogChannelMessageDelivered(ChannelMessageSentEventArgs eventArgs)
        {
            var template = this.Kernel.Resolve<IAdapterMessagingTemplate>();

            var payload = this.ResolveEnvelopePayload(eventArgs.Envelope);

            var msg = string.Format("Channel '{0}' message delivered. Contents: {1}",
                                    eventArgs.Channel,
                                    payload);

            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private string ResolveEnvelopePayload(IEnvelope envelope)
        {
            var payload = "{Not Specified}";

            if (envelope.Body.Payload == null)
                return payload;

            try
            {
                var serializer = this.Kernel.Resolve<ISerializationProvider>();

                if (serializer.IsInitialized())
                    payload = serializer.Serialize(envelope.Body.GetPayload<object>());
                else
                {
                    serializer.Scan(envelope.Body.GetPayload<object>().GetType().Assembly);
                    serializer.Initialize();
                    payload = serializer.Serialize(envelope.Body.GetPayload<object>());
                }
            }
            catch (Exception exception)
            {
                // just log the type name of the payload...
                payload = envelope.Body.GetPayload<object>().GetType().FullName;
            }

            return payload;

            
        }
    }
}