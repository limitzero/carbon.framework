namespace Carbon.Core.Stereotypes.For.MessageHandling.Aggregator.Impl
{
    public delegate bool CompletionStrategyAction(IEnvelope message);

    public delegate bool CorrelationStrategyAction(IEnvelope message);
}