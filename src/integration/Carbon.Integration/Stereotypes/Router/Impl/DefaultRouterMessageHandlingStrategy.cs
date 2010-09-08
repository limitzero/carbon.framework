using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;

namespace Carbon.Integration.Stereotypes.Router.Impl
{
    public class DefaultRouterMessageHandlingStrategy : AbstractRouterMessageHandlingStrategy
    {
        public override void DoRouterStrategy(IEnvelope message)
        {
            // save a copy of the original message:
            var originalMessage = message;

            var result = this.CurrentMethod.Invoke(this.CurrentInstance,
                                                   new object[] { message.Body.GetPayload<object>() });

            // set the output channel to the router's invalid choice channel:
            var outputChannelName = this.ExtractOutputChannel();
            if (!string.IsNullOrEmpty(outputChannelName))
                this.SetOutputChannel(outputChannelName);

            if (result.GetType() == typeof(string) && (result as string != string.Empty))
            {
                this.SetOutputChannel(result as string);
                base.OnRouterStrategyCompleted(result as string, message);
            }
            else
            {
                base.OnRouterStrategyCompleted(string.Empty, message);
                //this.OutputChannel.CreateProducer().Send(message);
            }
        }

        /// <summary>
        /// This will extract the output channel as indicated in the 
        /// <see cref="MessageEndpointAttribute">message end point attribute</see>
        /// annotation to the component.
        /// </summary>
        /// <returns></returns>
        private string ExtractOutputChannel()
        {
            var channelName = string.Empty;

            if (!(this.OutputChannel is NullChannel))
                return this.OutputChannel.Name;

            var attrs =
                this.CurrentInstance.GetType().GetCustomAttributes(
                    typeof(MessageEndpointAttribute), true);

            foreach (var attr in attrs)
            {
                if (attr.GetType() == typeof(MessageEndpointAttribute))
                {
                    channelName = ((MessageEndpointAttribute)attr).OutputChannel;
                    break;
                }
            }

            return channelName;
        }
    }
}