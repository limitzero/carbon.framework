using System.Collections.ObjectModel;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Aggregator.Impl
{
    /// <summary>
    /// Contract for handling a list of objects that should be aggregated into one object of the same type.
    /// </summary>
    /// <typeparam name="TAggregateItem">Type of the object to inspect for aggregation.</typeparam>
    public interface IAggregatorMessageHandlingStrategy<TAggregateItem> : IMessageHandlingStrategy
    {
        /// <summary>
        /// (Read-Only). Sets the method that will be examined for completing the aggregation.
        /// </summary>
        CompletionStrategyAction CompletionStrategy { get; }

        /// <summary>
        /// (Read-Only). Sets the method that will be examined for inspecting for correlated messages for the aggregation.
        /// </summary>
        CorrelationStrategyAction CorrelationStrategy { get; }

        /// <summary>
        /// This will set the action to call to see if the aggregation 
        /// can complete.
        /// </summary>
        /// <param name="completionStrategy"></param>
        void SetCompletionStrategy(CompletionStrategyAction completionStrategy);

        /// <summary>
        /// This will set the action to inspect to see if the messages are correlated.
        /// </summary>
        /// <param name="correlationStrategy"></param>
        void SetCorrelationStrategy(CorrelationStrategyAction correlationStrategy);

        /// <summary>
        /// This will return the list of items that are set for aggregation.
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<TAggregateItem> GetAccumulatedOutput();

    }
}