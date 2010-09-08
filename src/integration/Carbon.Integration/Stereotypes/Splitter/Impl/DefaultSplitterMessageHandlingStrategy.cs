using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Carbon.Core;

namespace Carbon.Integration.Stereotypes.Splitter.Impl
{
    /// <summary>
    /// Default splitting strategy that will support sending messages 
    /// to the output channel if the payload is derived from <seealso cref="IEnumerable"/>.
    /// </summary>
    public class DefaultSplitterMessageHandlingStrategy : AbstractSplitterMessageHandlingStrategy
    {
        /// <summary>
        /// This is the custom part of the splitter strategy where the message
        /// is split according to user-defined logic. When the message has been 
        /// split into a smaller part, the <see cref="AbstractSplitterMessageHandlingStrategy.OnMessageSplit"/> method
        /// should be called to forward the de-composed message part to the 
        /// configured output channel.
        /// </summary>
        /// <param name="message">Message received from the splitter method with a payload that can be iterated over.</param>
        public override void DoSplitterStrategy(IEnvelope message)
        {
            // need to do something here for splitting the message and 
            // send the result back on the output channel, for right
            // now just send each individual message back to the output
            // channel (note this is thread-blocking in behavior):
            if (typeof(IEnumerable).IsAssignableFrom(message.Body.GetPayload<object>().GetType()))
            {
                var iter = message.Body.GetPayload<IEnumerable>().GetEnumerator();

                while (iter.MoveNext())
                {
                    if (iter.Current != null)
                        base.OnMessageSplit(new Envelope(iter.Current));
                }
            }
        }
    }
}