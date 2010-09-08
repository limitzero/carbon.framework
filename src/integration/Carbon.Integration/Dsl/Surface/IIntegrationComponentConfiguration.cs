namespace Carbon.Integration.Dsl.Surface
{
    public interface IIntegrationComponentConfiguration
    {
        string InputChannel { get; set; }
        string OutputChannel { get; set; }
        string MethodName { get; set; }
    }
}