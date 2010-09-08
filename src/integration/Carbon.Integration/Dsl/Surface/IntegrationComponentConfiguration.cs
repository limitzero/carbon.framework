namespace Carbon.Integration.Dsl.Surface
{
    public class IntegrationComponentConfiguration : IIntegrationComponentConfiguration
    {
        public string Id { get; set; }
        public string InputChannel { get; set; }
        public string OutputChannel { get; set; }
        public string MethodName { get; set; }
    }
}