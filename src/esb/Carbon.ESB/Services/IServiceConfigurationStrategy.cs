using Carbon.Core.Builder;

namespace Carbon.ESB.Services
{
    /// <summary>
    /// Contract for all background services that the infrastructure will use. This will be the search 
    /// point for finding all of the concrete instances for configuring the service for use in messaging.
    /// </summary>
    public interface IServiceConfigurationStrategy
    {
        /// <summary>
        /// (Read-Write). The current component container instance for resolving or adding any resources.
        /// </summary>
        IObjectBuilder ObjectBuilder { get; set; }

        /// <summary>
        /// (Read-Write). The current messaging context for the service to send any messages.
        /// </summary>
        IMessageBus Bus { get; set; }

        /// <summary>
        /// This will configure the service based on the conventions needed and register it with the 
        /// <seealso cref="Carbon.ESB.Services.Registry.IBackgroundServiceRegistry">background service registry</seealso>to run 
        /// as an ancillary service to the framework.
        /// </summary>
        ContextBackgroundService Configure();
    }
}