using System;
using System.Collections.Generic;
using Carbon.Core.Stereotypes.For.MessageHandling;

namespace Carbon.Integration.Stereotypes.Accumulator.Impl
{
    public interface IAccumulatorMessageHandlingStrategy<T> : IMessageHandlingStrategy
    {
        /// <summary>
        /// (Read-Only). The listing of accumulated items.
        /// </summary>
        IList<T> AccumulatedItemsStorage { get; }

        /// <summary>
        /// (Read-Write). Flag to indicate whether or not to keep duplicate messages in the accumulated output.
        /// </summary>
        bool EnsureUniqueItems { get; set; }

        /// <summary>
        /// (Read-Write). The number of messages in the batch to accumulate (top limit of 256 messages per batch).
        /// </summary>
        int MessageBatchSize { get; set; }
    }
}