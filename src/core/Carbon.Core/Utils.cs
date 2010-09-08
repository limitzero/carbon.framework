using System;
using System.Collections.Specialized;
using Carbon.Core.Adapter.Strategies.Retry;
using Carbon.Core.Builder;
using Carbon.Core.Adapter.Template;

namespace Carbon.Core
{
    public class Utils
    {
        static Utils()
        {
        }

        public static NameValueCollection CreateNameValuePairsFromUri(string uri)
        {
            var nameValueCollection = new NameValueCollection();

            if (!uri.Contains("?"))
                return nameValueCollection;

            var queryString = uri.Split(new char[] { '?' })[1];

            if (!string.IsNullOrEmpty(queryString))
            {
                var nameValuePairs = queryString.Split(new char[] { '&' });

                foreach (var nameValuePair in nameValuePairs)
                {
                    var name = nameValuePair.Split(new char[] { '=' })[0];
                    var value = nameValuePair.Split(new char[] { '=' })[1];

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                        continue;

                    try
                    {
                        nameValueCollection.Add(name, value);
                    }
                    catch (Exception exception)
                    {
                    }

                }
            }

            return nameValueCollection;
        }

        /// <summary>
        /// This is the global retry logic for all output adapters.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="action"></param>
        /// <param name="destination"></param>
        /// <param name="message"></param>
        /// <param name="strategy"></param>
        public static void Retry(IObjectBuilder builder, Action<string, IEnvelope> action, 
            string destination, IEnvelope message, IRetryStrategy strategy)
        {
            var isMessageDelivered = false;
            Exception retryException = null;

            if (strategy == null)
                strategy = new RetryStrategy(2, 1);

            for (var i = 0; i < strategy.MaxRetries; i++)
            {
                if (isMessageDelivered)
                    break;

                try
                {
                    action.Invoke(destination, message);
                    isMessageDelivered = true;
                }
                catch (Exception exception)
                {
                    retryException = exception;

                    if (strategy.WaitInterval > 0)
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(strategy.WaitInterval));

                    continue;
                }
            }

            if (!isMessageDelivered)
            {
                var contents = message.Body.GetPayload<object>().GetType().FullName;
                var msg = "The message '" + contents + "' could not be delivered to the destination of " + destination + ". ";
                if (retryException != null)
                    msg = msg + "Reason: " + retryException.ToString();

                if (!string.IsNullOrEmpty(strategy.FailureDeliveryUri))
                {
                    // send the mesage to the location:
                    builder.Resolve<IAdapterMessagingTemplate>().DoSend(new Uri(strategy.FailureDeliveryUri), message);
                    // note the message in the log:
                    builder.Resolve<IAdapterMessagingTemplate>().DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI), new Envelope(msg));
                }
               
                // need to throw for consistency:
                throw new Exception(msg);
                
            }

        }
    }
}