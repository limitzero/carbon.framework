using System;
using System.Data.OleDb;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Adapter;

namespace Carbon.Sql.Adapter
{
    /// Uri used to create the channel end point  =>  sql://{context provider reference}
    ///  The query should passed as a message to the adapter
    public class SqlOutputChannelAdapter : AbstractOutputChannelAdapter
    {
        public SqlOutputChannelAdapter(IObjectBuilder builder): base (builder)
        {
          
        }

        public override void DoSend(IEnvelope envelope)
        {
            this.SendMessage(envelope);
        }

        private void SendMessage(IEnvelope message)
        {
            try
            {
                SubmitMessage(this.Uri, message);
            }
            catch (Exception ex)
            {
                Utils.Retry(base.ObjectBuilder, (arg1, arg2) => this.SubmitMessage(this.Uri, message),
                    this.Uri, message, base.RetryStrategy);
            }
        }

        private void SubmitMessage(string destination, IEnvelope message)
        {
            var statement = string.Empty;
            var isCommandFound = false;

            try
            {
                // first, the body of the message must be a textual representation of a SQL statement:
                if(message.Body.GetPayload<object>().GetType() != typeof(string))
                    return;

                statement = message.Body.GetPayload<string>();

                // second, since this is the "producing" side of SQL messages, only INSERT, UPDATE and DELETE are accepted:       
                if (statement.Trim().ToLower().StartsWith("update") ||
                    statement.Trim().ToLower().StartsWith("delete") ||
                    statement.Trim().ToLower().StartsWith("insert"))
                    isCommandFound = true;

                if (!isCommandFound)
                    return;
            }
            catch (Exception exception)
            {
                return;
            }

            try
            {
                var provider = SqlAdapterUtils.CreateContextProvider(base.ObjectBuilder, this.Uri);

                if(provider == null)
                    return;

                using (var cxn = provider.GetConnection())
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

                            message.Header.SequenceSize = rowsAffected;
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