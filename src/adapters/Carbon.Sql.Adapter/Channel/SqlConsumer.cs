using System;
using System.Data;
using System.Data.Common;
using System.Transactions;
using System.Data.OleDb;
using Carbon.Integration.Core;
using Carbon.Integration.Transports.Impl;

namespace Carbon.Integration.Channels.Impl.Sql
{
    /// <summary>
    /// The sql consumer can only process SELECT * statements as it should "consume" (read "query")
    /// the data store for messages and transpose them into a IDataStoreMessage for transport
    /// </summary>
    public class SqlConsumer : BaseConsumer
    {
        private SqlChannel m_sql_channel = null;

        public SqlConsumer(SqlChannel channel, IContext context)
            : base(channel, context, new NullTransport())
        {
            m_sql_channel = channel;
        }

        public override IEnvelope DoReceive()
        {
            IEnvelope receivedMessage = new NullEnvelope();

            // since this is the "consuming" side of SQL messages, only SELECT is accepted:
            if (!m_sql_channel.ContextProvider.QueryText.Trim().ToLower().StartsWith("select"))
                return receivedMessage;

            try
            {
                using (var cxn = m_sql_channel.ContextProvider.GetConnection())
                using (var cmd = new OleDbCommand(m_sql_channel.ContextProvider.QueryText, cxn))
                {
                    var dataSet = new DataSet();
                    var adapter = new OleDbDataAdapter(cmd);

                    cxn.Open();

                    using (var txn = cxn.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = txn;
                            adapter.Fill(dataSet);

                            //var reader = cmd.ExecuteReader();

                            receivedMessage = new Envelope(dataSet);

                            if (dataSet.Tables.Count > 0)
                                if (dataSet.Tables[0].Rows.Count > 0)
                                    receivedMessage.Header.SequenceSize = dataSet.Tables[0].Rows.Count;

                            txn.Commit();
                        }
                        catch (Exception exception)
                        {
                            txn.Rollback();
                            throw;
                        }

                    }
                }

            }
            catch (Exception exception)
            {
                throw;
            }


            return receivedMessage;
        }
    }


}