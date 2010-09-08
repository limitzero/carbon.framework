using System;
using System.Collections;
using System.Data;
using Carbon.Integration.Core;
using Carbon.Integration.Channels.Impl.Sql.ContextProvider;

namespace Carbon.Integration.Channels.Impl.Sql
{
    //http://www.carlprothman.net/Default.aspx?tabid=81 {sample connnection strings for all types of providers}

    /// <summary>
    /// The sql channel allows for the messages to be produced (i.e. "changed -> inserted, updated or deleted") or 
    /// consumed (i.e. "received -> selected") over a dedicated sql datastore.
    /// 
    /// Uri used to create the channel end point : 
    /// sql://{context provider reference}/?concurrency={1..10}&frequency={1..10}&scheduled={1..10}
    /// </summary>
    public class SqlChannel : BaseChannel
    {
        private const string m_scheme = "sql://";
        private IProducer m_producer = null;
        private IConsumer m_consumer = null;

        public ISqlContextProvider ContextProvider
        {
            get;
            private set;
        }

        public SqlChannel()
        {
            base.Scheme = m_scheme;
        }

        public override IEnvelope DoReceive()
        {
            return m_consumer.Receive();
        }

        public override void DoSend(IEnvelope envelope)
        {
            m_producer.Send(envelope);
        }

        public override void CreateEndpoint(string endpointUri)
        {
            // uri scheme for this channel: 

            // sql://{context provider reference}/?concurrency={1..10}&frequency={0..10}

            var channelUri = new Uri(endpointUri);
            var nameValuePairs = ChannelUtils.CreateNameValuePairsFromUri(endpointUri);

            var contextProvider = this.Context.Kernel.Resolve(channelUri.Host, new Hashtable());
            Create((ISqlContextProvider) contextProvider);

        }

        /// <summary>
        /// This will create the channel based on the SQL context information for 
        /// connectivity and the initial query statement for polling.
        /// </summary>
        /// <param name="provider"></param>
        public void Create(ISqlContextProvider provider)
        {
            ContextProvider = provider;

            // use the custome producer/consumer for the SQL channel:

            m_producer = new SqlProducer(this, base.Context);
            m_consumer = new SqlConsumer(this, base.Context);
        }

    }
}