using System;
using System.Data;
using System.Data.OleDb;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Adapter;

namespace Carbon.Sql.Adapter
{
    /// Uri used to create the channel end point  =>  sql://{context provider reference}
    ///  The query should passed as a message to the adapter
    public class SqlInputChannelAdapter : AbstractInputChannelAdapter
    {
        public SqlInputChannelAdapter(IObjectBuilder builder) : base(builder)
        {
          
        }

        public override byte[] ExtractMessageContents()
        {
            return new byte[] {};           
        }

        public override IEnvelopeHeader CreateMessageHeader()
        {
            return new EnvelopeHeader();
        }

        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            // create the header and payload as indicated but set the custom message for receipt
            // to be the envelope with the dataset containing the results, the base class will inspect
            // the current message for receive instead of the tuple:
            var payload = ExtractMessageContents();
            var header = CreateMessageHeader();

            var envelope = this.ReceiveMessage(this.Uri);

            this.SetMessageForReceive(envelope);

            return new Tuple<IEnvelopeHeader, byte[]>(header, payload);
        }

        private IEnvelope ReceiveMessage(string location)
        {
            IEnvelope receivedMessage = new NullEnvelope();

            // since this is the "consuming" side of SQL messages, only SELECT is accepted:
            var provider = SqlAdapterUtils.CreateContextProvider(ObjectBuilder, this.Uri);

            if (provider == null)
                return receivedMessage;

            if (!provider.QueryText.Trim().ToLower().StartsWith("select"))
                return receivedMessage;

            try
            {
                using (var cxn = provider.GetConnection())
                using (var cmd = new OleDbCommand(provider.QueryText, cxn))
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