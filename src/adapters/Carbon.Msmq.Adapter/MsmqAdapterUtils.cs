using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Security.Principal;

namespace Carbon.Msmq.Adapter
{
    public class MsmqAdapterUtils
    {
        static MsmqAdapterUtils()
        {
        }

        public static void CreateTransactonalQueue(string path)
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            if (!MessageQueue.Exists(path))
            {
                MessageQueue.Create(path, true);
            }
            else
            {
                var queue = new MessageQueue(path);
                if (!queue.Transactional)
                    throw new ArgumentException("The queue [" + path + "] must be transactional.");
            }
        }

        public static string RetreiveLocationFromProtocolUri(string location)
        {
            string path = string.Empty;
            string result = "FormatName:Direct=OS:{0}{1}";

            var msmqUri = new Uri(location);
            var absolutePath = msmqUri.AbsolutePath;

            if (msmqUri.Host.ToLower() == "localhost")
            {
                if (absolutePath.EndsWith(@"/"))
                    absolutePath = absolutePath.Substring(0, absolutePath.Length - 1);

                path = string.Concat(".", absolutePath.Replace("/", @"\"));
            }
            else
            {
                var parts = absolutePath.Split(new char[] { '/' });
                path = string.Format(result, msmqUri.Host, parts[1].Trim());
            }

            return path;

        }

        public static string ResolveUriPathForQueueFromCommonName(string path)
        {
            var uri = "msmq://{0}/private$/{1}";
            var serverName = "localhost";
            var queueName = string.Empty;

            // uses the form .\private$\{queue name} to create msmq://{servername}/private$/{queuename}
            var forwarSlash = @"\";
            var items = path.Split(forwarSlash.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(items[0]))
                if (items[0].Trim() != ".")
                    serverName = items[0].Trim();

            if (!string.IsNullOrEmpty(items[2]))
                queueName = items[2].Trim();

            uri = string.Format(uri, serverName, queueName);

            return uri;
        }

        public static MessageQueueTransactionType GetTransactionTypeForReceive(bool isTransactional)
        {
            if (isTransactional)
                return MessageQueueTransactionType.Automatic;
            return MessageQueueTransactionType.None;
        }

        public static MessageQueueTransactionType GetTransactionTypeForSend(bool isTransactional)
        {
            if (isTransactional)
                return MessageQueueTransactionType.Automatic;

            return MessageQueueTransactionType.Single;
        }
    }
}