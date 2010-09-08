using System;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface;
namespace Carbon.Integration.Configuration.Surface
{
    /// <summary>
    ///  Empty surface used by the configuration to dyamically create and register
    /// in the integration context.
    /// </summary>
    public class DefaultSurface : AbstractIntegrationComponentSurface
    {
        public DefaultSurface(IObjectBuilder builder) : base(builder)
        {

        }

        public override void BuildReceivePorts()
        {
            
        }

        public override void BuildCollaborations()
        {
          
        }

        public override void BuildSendPorts()
        {
           
        }

        public override void BuildErrorPort()
        {
            
        }
    }
}