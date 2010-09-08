using System;
using System.Timers;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Impl.Null;

namespace Carbon.Core.Adapter.Template
{
    /// <summary>
    /// Concrete instance that all adapters will use for sending and receiving messages
    /// </summary>
    public class AdapterMessagingTemplate : IAdapterMessagingTemplate
    {
        private readonly IAdapterFactory m_adapter_factory;
        private string m_location = string.Empty;
        private bool m_is_completed = false;
        private IEnvelope m_response_message = new NullEnvelope();

        public AdapterMessagingTemplate(IAdapterFactory adapterFactory)
        {
            m_adapter_factory = adapterFactory;
        }

        public void DoSend(Uri locationSemantic, IEnvelope message)
        {
            m_location = locationSemantic.OriginalString;
            var outputAdapter = m_adapter_factory.BuildOutputAdapterFromUri(locationSemantic.OriginalString);

            if(outputAdapter is NullOutputChannelAdapter)
                return;

            outputAdapter.Uri = locationSemantic.OriginalString;
            outputAdapter.DoStartActivities();
            outputAdapter.DoSend(message);
            outputAdapter.DoStopActivities();

            this.m_response_message = outputAdapter.ResponseMessage;
        }

        public void DoSend(Uri locationSemantic, IEnvelope message, int timeout)
        {
            var timer = new Timer(timeout * 1000);

            try
            {
                m_is_completed = false;

                timer.Elapsed += Timer_Elasped;
                timer.Start();

                this.DoSend(locationSemantic, message);
                m_is_completed = true;

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

        public IEnvelope DoReceive(Uri locationSemantic)
        {
            IEnvelope envelope = new NullEnvelope();
            m_location = locationSemantic.OriginalString;

            var inputChannelAdapter = m_adapter_factory.BuildInputAdapterFromUri(locationSemantic.OriginalString);

            if (inputChannelAdapter is NullInputChannelAdapter)
                return envelope;

            inputChannelAdapter.Uri = m_location;

            inputChannelAdapter.DoStartActivities();
            envelope = inputChannelAdapter.Receive();

            inputChannelAdapter.DoStopActivities();

            return envelope;
        }

        public IEnvelope DoReceive(Uri locationSemantic, int timeout)
        {
            IEnvelope message = new NullEnvelope();

            var timer = new Timer(timeout * 1000);

            try
            {
                m_is_completed = false;

                timer.Elapsed += Timer_Elasped;
                timer.Start();

                message = this.DoReceive(locationSemantic);
                m_is_completed = true;

                timer.Stop();
            }
            catch (Exception exception)
            {
                timer.Stop();
                throw;
            }
            finally
            {
                if(timer.Enabled)
                    timer.Stop();

                timer.Elapsed -= Timer_Elasped;
                timer.Dispose();
            }

            return message;
        }

        public IEnvelope DoSendAndReceive(Uri sendLocation, Uri receiveLocation, IEnvelope message)
        {
            IEnvelope retval = new NullEnvelope();

            // use the target adapter for sending and the source adapter for receiving:
            m_location = sendLocation.OriginalString;
            this.DoSend(sendLocation, message);

            if (m_response_message is NullEnvelope)
            {
                m_location = receiveLocation.OriginalString;
                retval = this.DoReceive(receiveLocation);
            }
            else
            {
                retval = this.m_response_message;
            }

            return retval;
        }

        public IEnvelope DoSendAndReceive(Uri sendLocation, Uri receiveLocation, IEnvelope message, int timeout)
        {
            IEnvelope retval = new NullEnvelope();

            var timer = new Timer(timeout * 1000);

            try
            {
                m_is_completed = false;

                timer.Elapsed += Timer_Elasped;
                timer.Start();

                retval = this.DoSendAndReceive(sendLocation, receiveLocation, message);
                m_is_completed = true;

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

            return retval;
        }

        private void Timer_Elasped(object sender, ElapsedEventArgs e)
        {
            if (!m_is_completed)
                throw new Exception(
                    "The message could not be delivered from the location '" + m_location + "' within the time alloted.");
        }
    }
}