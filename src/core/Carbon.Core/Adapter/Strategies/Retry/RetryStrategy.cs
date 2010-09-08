using System;

namespace Carbon.Core.Adapter.Strategies.Retry
{
    public class RetryStrategy : IRetryStrategy
    {
        public int MaxRetries { get; set; }
        public int WaitInterval { get; set; }
        public string FailureDeliveryUri { get; set; }

        public RetryStrategy()
            :this(2, 1, string.Empty)
        {
        }

        public RetryStrategy(int maxRetries, int waitInterval)
            :this(maxRetries, waitInterval, string.Empty)
        {
        }

        public RetryStrategy(int maxRetries, int waitInterval, string failureDeliveryUri)
        {
            MaxRetries = maxRetries;
            WaitInterval = waitInterval;
            FailureDeliveryUri = failureDeliveryUri;
        }
    }
}