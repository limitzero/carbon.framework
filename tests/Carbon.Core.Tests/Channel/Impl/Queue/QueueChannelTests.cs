using Carbon.Core.Channel.Impl.Queue;
using Xunit;

namespace Carbon.Core.Tests.Channel.Impl.Queue
{
    public class when_a_message_is_sent_via_the_queue_channel
    {
        private QueueChannel m_channel = null;
        private string m_channel_name = "send_tests";

        public when_a_message_is_sent_via_the_queue_channel()
        {
            m_channel = new QueueChannel(m_channel_name);    
        }

        [Fact]      
        public void it_will_store_the_message_in_the_internal_storage_using_the_basic_transport_services()
        {
            var message = new Envelope("test message for send...");
            m_channel.Send(message);
            Assert.Equal(1, m_channel.GetMessages().Count);
        }

    }

    public class when_a_message_is_received_using_the_queue_channel
    {
        private QueueChannel m_channel = null;
        private string m_channel_name = "receive_tests";

        public when_a_message_is_received_using_the_queue_channel()
        {
            m_channel = new QueueChannel(m_channel_name);    
        }

        [Fact]
        public void it_will_return_a_null_envelope_message_if_no_messages_are_found_at_the_storage_location()
        {
            var envelope = m_channel.Receive(2);
            Assert.Equal(typeof(NullEnvelope), envelope.GetType());
        }

        [Fact]
        public void it_will_return_a_envelope_message_at_the_storage_location()
        {
            var envelope = new Envelope("test message for receive...");
            m_channel.Send(envelope);

            var fromStorage = m_channel.Receive(2);

            Assert.Equal(envelope, fromStorage);
        }

    }
}