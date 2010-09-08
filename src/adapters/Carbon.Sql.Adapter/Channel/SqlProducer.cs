using System;
using System.Data.OleDb;
using System.Transactions;
using Carbon.Integration.Core;
using Carbon.Integration.Transports.Impl;

namespace Carbon.Integration.Channels.Impl.Sql
{
    /// <summary>
    /// The sql producer can only process INSERT, UPDATE, and DELETE statements as it should "produce" (read "send commands")
    /// results to the data store.
    /// </summary>
    public class SqlProducer : BaseProducer
    {
        private SqlChannel m_sql_channel = null;

        public SqlProducer(SqlChannel channel, IContext context) 
            : base(channel, context, new NullTransport())
        {
            m_sql_channel = channel;
        }

        public override void DoSend(IEnvelope message)
        {
            IEnvelope receivedMessage = new NullEnvelope();
            var statement = string.Empty;
            var isCommandFound = false;

            try
            {
                // first, the body of the message must be a textual representation of a SQL statement:
                statement = message.Body.GetPayload<string>();

                // second, since this is the "producing" side of SQL messages, only INSERT, UPDATE and DELETE are accepted:       
                if (statement.Trim().ToLower().StartsWith("update") || 
                    statement.Trim().ToLower().StartsWith("delete") ||
                    statement.Trim().ToLower().StartsWith("insert"))
                    isCommandFound = true;

                if(!isCommandFound)
                    return;
            }
            catch (Exception exception)
            {
                return;
            }

            try
            {
                using (var cxn = m_sql_channel.ContextProvider.GetConnection())
                using (var cmd = new OleDbCommand(statement, cxn))
                {
                    cxn.Open();

                    using (var txn = cxn.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = txn;
                            var rowsAffected = cmd.ExecuteNonQuery();

                            txn.Commit();

                            receivedMessage = new Envelope(statement);
                            receivedMessage.Header.SequenceSize = rowsAffected;
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

        }

    }
}
