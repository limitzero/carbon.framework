using System;
using System.Collections.Generic;
using System.Timers;
using Carbon.Core.Exceptions;

namespace Carbon.Core.Channel
{

    public delegate void ChannelMessageSentDelegate(ChannelMessageSentEventArgs eventArgs);
    public delegate void ChannelMessageReceivedDelegate(ChannelMessageReceivedEventArgs eventArgs);

    /// <summary>
    /// Core concept for moving messages between locations.
    /// </summary>
    public abstract class AbstractChannel
    {
        /// <summary>
        /// This is the message that is manually set for retrieval for testing the channel.
        /// </summary>
        private Envelope m_message_to_receive = new NullEnvelope();

        private List<ChannelMessageSentDelegate> m_message_sent_actions;
        private List<ChannelMessageReceivedDelegate> m_message_received_actions;

        private bool m_is_duration_exceeded = false;

        #region -- events --

        /// <summary>
        /// Event that is triggered when the message is sent to a channel location.
        /// </summary>
        public event EventHandler<ChannelMessageSentEventArgs> MessageSent;

        /// <summary>
        /// Event that is triggered when the message is received from a channel location.
        /// </summary>
        public event EventHandler<ChannelMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Event that is triggered when the channel expiriences any error.
        /// </summary>
        public event EventHandler<ChannelErrorEncounteredEventArgs> ChannelError;

        #endregion

        #region -- properties --

        /// <summary>
        /// (Read-Only). The name of the channel.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// (Read-Write). Flag to indicate whether or not the channel can store duplicate messages (default = true => dups allowed)
        /// </summary>
        public bool IsIdempotent
        {
            get;
            set;
        }

        #endregion

        protected AbstractChannel()
        {
            this.IsIdempotent = true;
            this.m_message_received_actions = new List<ChannelMessageReceivedDelegate>();
            this.m_message_sent_actions = new List<ChannelMessageSentDelegate>();
        }

        /// <summary>
        /// This will set the name of the channel (name must be unique).
        /// </summary>
        /// <param name="name">Name of the channel</param>
        /// <exception cref="ArgumentNullException">For null, empty or blank (i.e. " ") channel name.</exception>
        public void SetChannelName(string name)
        {
            try
            {
                var exception = new ArgumentNullException("name",
                                                          "The name of the channel can not be empty or null.");

                if (string.IsNullOrEmpty(name))
                    throw exception;

                if (name.Trim() == string.Empty)
                    throw exception;

                this.Name = name;
            }
            catch (Exception exception)
            {
                if (!OnChannelError(exception))
                    throw;
            }


            this.Name = name;
        }

        /// <summary>
        /// Extension point for channel implementations to send messages to a location.
        /// </summary>
        /// <returns></returns>
        public virtual void DoSend(IEnvelope envelope)
        {
        }

        /// <summary>
        /// This will send a message to the target location for subsequent 
        /// processing by another agent.
        /// </summary>
        /// <param name="envelope"></param>
        public void Send(IEnvelope envelope)
        {
            try
            {
                // send the message to the location:
                DoSend(envelope);

                // we sent the message, trigger the listeners:
                OnMessageSent(envelope);
            }
            catch (Exception exception)
            {
                if (!OnChannelError(exception))
                    throw;
            }
        }

        /// <summary>
        /// Extension point for channel implementations to retreive messages from a location.
        /// </summary>
        /// <returns></returns>
        public virtual IEnvelope DoReceive()
        {
            return null;
        }

        /// <summary>
        /// This will poll a location idefinitely for the presence of a message.
        /// </summary>
        /// <returns></returns>
        public IEnvelope Receive()
        {
            return this.Receive(24 * 60 * 60); // poll for one whole day (in seconds)!!
        }

        /// <summary>
        /// This will poll a location for a message over a given interval in seconds.
        /// </summary>
        /// <param name="timeout">Interval in seconds (positive) to poll the location for a message.</param>
        /// <returns>
        /// 
        /// </returns>
        /// <exception cref="ReceiveTimeoutPeriodExeceededException">Received timeout period exception.</exception>
        public IEnvelope Receive(int timeout)
        {
            IEnvelope envelope = new NullEnvelope();
            Timer timer = null;

            try
            {
                if (timeout < 0)
                    throw new ArgumentNullException("timeout",
                                                    "The timeout value must be a positive integer representing the interval in seconds.");

                timer = new Timer(timeout * 1000);
                timer.Elapsed += TimerElasped;

                while (true)
                {
                    timer.Start();

                    // retreive the enveloped message from the location:
                    envelope = DoReceive();

                    if (envelope != null)
                        break;

                    if (m_is_duration_exceeded)
                        break;

                    System.Threading.Thread.Sleep(100);
                }

                timer.Elapsed -= TimerElasped;
                timer.Stop();

                if (m_message_to_receive.GetType() != typeof(NullEnvelope))
                {
                    OnMessageReceived(m_message_to_receive);
                    return m_message_to_receive;
                }

                if (m_is_duration_exceeded)
                    throw new ReceiveTimeoutPeriodExeceededException();

                // we have the message, trigger the listeners:
                OnMessageSent(envelope);
            }

            catch (Exception exception)
            {
                if (!OnChannelError(exception))
                    throw;
            }

            return envelope;
        }


        /// <summary>
        /// This will set the message to be retreived from the 
        /// channel (for test only).
        /// </summary>
        /// <param name="envelope"></param>
        public void SetMessage(Envelope envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException("envelope", "The envelope set for the channel must not be null. Try passing a new instance of the NullEnvelope object instead.");

            m_message_to_receive = envelope;
        }


        /// <summary>
        /// This will register the set of actions to be invoked when the message has been sent to the target.
        /// </summary>
        /// <param name="messageSentActions"></param>
        public void RegisterActionsOnMessageSend(params ChannelMessageSentDelegate[] messageSentActions)
        {
            if (m_message_sent_actions.Count > 0)
                m_message_sent_actions.AddRange(messageSentActions);
            else
            {
                m_message_sent_actions = new List<ChannelMessageSentDelegate>(messageSentActions);
            }
        }

        /// <summary>
        /// This will register the set of actions to be involed when the message has been received from the source.
        /// </summary>
        /// <param name="messageReceivedActions"></param>
        public void RegisterActionsOnMessageReceived(params ChannelMessageReceivedDelegate[] messageReceivedActions)
        {
            if (this.m_message_received_actions.Count > 0)
                m_message_received_actions.AddRange(messageReceivedActions);
            else
            {
                m_message_received_actions = new List<ChannelMessageReceivedDelegate>(messageReceivedActions);
            }
        }

        private void TimerElasped(object sender, ElapsedEventArgs e)
        {
            m_is_duration_exceeded = true;
        }


        private void OnMessageSent(IEnvelope envelope)
        {
            EventHandler<ChannelMessageSentEventArgs> evt = this.MessageSent;
            if (evt != null)
                evt(this, new ChannelMessageSentEventArgs(envelope, this.Name));

            if (m_message_sent_actions.Count > 0)
                m_message_sent_actions.ForEach(x => x.Invoke(new ChannelMessageSentEventArgs(envelope, this.Name)));
        }

        private void OnMessageReceived(IEnvelope envelope)
        {
            EventHandler<ChannelMessageReceivedEventArgs> evt = this.MessageReceived;
            if (evt != null)
                evt(this, new ChannelMessageReceivedEventArgs(envelope, this.Name));

            if (m_message_received_actions.Count > 0)
                m_message_received_actions.ForEach(x => x.Invoke(new ChannelMessageReceivedEventArgs(envelope, this.Name)));
        }

        private bool OnChannelError(Exception exception)
        {
            EventHandler<ChannelErrorEncounteredEventArgs> evt = this.ChannelError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new ChannelErrorEncounteredEventArgs(exception));

            return isHandlerAttached;
        }
    }
}