using System;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Gateway.Impl
{
    /// <summary>
    /// Holds the configuration information for the gateway implementation.
    /// </summary>
    public class GatewayDefinition : IGatewayDefinition
    {
        public string Id { get; set; }
        public string Contract { get; set; }
        public string Method { get; set; }
        public string RequestChannel { get; set; }
        public string ReplyChannel { get; set; }
        public int ReceiveTimeout { get; set; }
        public string Service { get; set;}
       
        public GatewayDefinition()
        {
            this.ReceiveTimeout = 10;
        }
    }
}