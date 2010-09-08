using System.Collections.Generic;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Consumes.Impl
{
    /// <summary>
    /// Contract for handling the <seealso cref="ConsumesAttribute">consumes</seealso>
    /// annotation on a method for a <seealso cref="MessageEndpointAttribute">message endpoint</seealso>.
    /// </summary>
    public interface IConsumesMessageHandlingStrategy : IMessageHandlingStrategy
    {
    }
}