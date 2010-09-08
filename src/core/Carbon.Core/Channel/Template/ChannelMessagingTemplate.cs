using System;
using System.Threading;
using System.Timers;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Null;
using Timer=System.Timers.Timer;

namespace Carbon.Core.Channel.Template
{
    /// <summary>
    /// Concrete instance that all channels will use for sending and receiving messages
    /// </summary>
    public class ChannelMessagingTemplate : IChannelMessagingTemplate
    {
        private string m_location = string.Empty;
        private readonly IChannelRegistry _registry;
        private bool _is_completed;

        public ChannelMessagingTemplate(IChannelRegistry registry)
        {
            _registry = registry;
        }

        public void DoSend(string locationSemantic, IEnvelope message)
        {
            var currentChannel = _registry.FindChannel(locationSemantic);

            if(currentChannel is NullChannel) 
                throw new Exception(string.Format("The channel '{0}' could not be found for sending a message.", locationSemantic));

            currentChannel.Send(message);
        }

        public void DoSend(string locationSemantic, IEnvelope message, int timeout)
        {
            var timer = new Timer(timeout * 1000);
            m_location = locationSemantic;

            try
            {
                _is_completed = false;

                timer.Elapsed += Timer_Elasped;
                timer.Start();

                this.DoSend(locationSemantic, message);
                _is_completed = true;

                timer.Stop();
            }
            catch (Exception exception)
            {
                timer.Stop();
                throw;
            }
            finally
            {
                if (timer.Enabled)
                    timer.Stop();

                timer.Elapsed -= Timer_Elasped;
                timer.Dispose();
            }

        }

        public IEnvelope DoReceive(string locationSemantic)
        {
            return this.DoReceive(locationSemantic, 1);
        }

        public IEnvelope DoReceive(string locationSemantic, int timeout)
        {
            var currentChannel = _registry.FindChannel(locationSemantic);

            if (currentChannel is NullChannel)
                throw new Exception(string.Format("The channel '{0}' could not be found for receiving a message.", locationSemantic));

            var envelope = currentChannel.Receive(timeout);
            return envelope;
        }

        public IEnvelope DoSendAndReceive(string sendLocation, string receiveLocation, IEnvelope message)
        {
            return this.DoSendAndReceive(sendLocation, receiveLocation, message, 1);
        }

        public IEnvelope DoSendAndReceive(string sendLocation, string receiveLocation, IEnvelope message, int timeout)
        {
            IEnvelope retval = new NullEnvelope();

            var wait = new ManualResetEvent(false);

            try
            {
                // send the message:
                this.DoSend(sendLocation, message);
                wait.WaitOne(TimeSpan.FromSeconds(timeout));
                wait.Set();

                // retrieve the message:
                retval = this.DoReceive(receiveLocation);

            }
            catch (Exception exception)
            {
                throw;
            }

            return retval;
        }

        private void Timer_Elasped(object sender, ElapsedEventArgs e)
        {
            if (!_is_completed)
                throw new Exception(
                    "The message could not be delivered to the location '" + m_location + "' within the time alloted.");
        }
    }
}