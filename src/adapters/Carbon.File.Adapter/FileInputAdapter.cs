using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;
using Carbon.Core.Internals;

namespace Carbon.File.Adapter
{
    /// <summary>
    /// Adapter for taking files from a location and loading them onto a channel.
    /// </summary>
    public class FileInputAdapter :  AbstractInputChannelAdapter
    {
        private ReaderWriterLock m_reader_writer_lock = new ReaderWriterLock();
        private DirectoryInfo m_message_queue = null;
        private static object m_file_access_lock = new object();
        private string m_received_message = null;
        private bool m_delete_on_retrieval = true;

        /// <summary>
        /// (Read-Write). The suffix that is appended to the file when 
        /// it is picked up for processing.
        /// </summary>
        public string ProcessedFileSuffix { get; set; }

        public FileInputAdapter(IObjectBuilder builder)
            :base(builder)
        {
            
        }

        public override void DoStartActivities()
        {
            var pairs = Utils.CreateNameValuePairsFromUri(this.Uri);
            m_delete_on_retrieval = false;

            bool.TryParse(pairs["delete"], out m_delete_on_retrieval);

        }

        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            var contents = this.ExtractMessageContents();
            var header = this.CreateMessageHeader();
            var tuple = new Tuple<IEnvelopeHeader, byte[]>(header, contents);
            return tuple;
        }

        public override byte[] ExtractMessageContents()
        {
            byte[] retval = null;
            var source = FileAdapterUtils.RetreiveLocationFromProtocolUri(base.GetScheme(), this.Uri);

            m_message_queue = new DirectoryInfo(source);
            m_reader_writer_lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                var fileName = string.Empty;

                if (TryPeek())
                {

                    this.m_received_message = m_message_queue.GetFiles()[0].FullName;

                    if (string.IsNullOrEmpty(this.m_received_message)) return retval;

                    retval = GetFileContents(this.m_received_message);

                    if (retval != null)
                        new TransactionContext(() => DeleteMessage(this.m_received_message), this.IsTransactional);
                }

            }
            catch (IOException iex)
            {
                // nothing to do here...let the other thread have the file
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                m_reader_writer_lock.ReleaseReaderLock();
            }

            return retval;
        }

        public override IEnvelopeHeader CreateMessageHeader()
        {
            IEnvelopeHeader header = new EnvelopeHeader();

            if(string.IsNullOrEmpty(this.m_received_message)) return header;

            var identifiers = FileAdapterUtils.GetDefaultFileIdentifiers(this.m_received_message);

            // message identifier:
            if(!string.IsNullOrEmpty(identifiers.Item1))
               header.SetMessageId(identifiers.Item1);
            else
            {
                var messageId = string.Format("{0}-{1}", "FILE", System.Guid.NewGuid().ToString());
                header.SetMessageId(messageId);
            }

            // correlation identifier:
            if (!string.IsNullOrEmpty(identifiers.Item2))
                header.CorrelationId = identifiers.Item2;

            // store the file name of the message received:
            header.AddHeaderItem(FileHeaders.FileName.ToString(), this.m_received_message);

            return header;
        }

        private bool TryPeek()
        {
            bool needToHandle = false;

            try
            {
                needToHandle = m_message_queue.GetFiles().Length > 0;
            }
            catch (IOException iex)
            {

            }
            catch (Exception exception)
            {

            }

            return needToHandle;
        }

        private byte[] GetFileContents(string fileName)
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            
            byte[] retval = null;

            lock (m_file_access_lock)
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (TextReader reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();
                    retval = ASCIIEncoding.ASCII.GetBytes(contents);
                }

                return retval;
            }
        }

        private void DeleteMessage(string filename)
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            //if(!m_delete_on_retrieval) return; 

            var info = new FileInfo(filename);

            lock (info)
            {
                info.Delete();
            }
        }

    }
}