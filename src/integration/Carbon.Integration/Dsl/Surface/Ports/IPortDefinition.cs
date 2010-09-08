namespace Carbon.Integration.Dsl.Surface.Ports
{
    public interface IPortDefinition
    {
        string Channel { get; }
        string Uri { get; }
        int Concurrency { get; }
        int Frequency { get; }
        int Schedule { get; }
    }
}