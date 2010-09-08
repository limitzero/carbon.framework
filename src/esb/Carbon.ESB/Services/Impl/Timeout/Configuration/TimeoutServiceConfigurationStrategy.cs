using Carbon.Core.Builder;

namespace Carbon.ESB.Services.Impl.Timeout.Configuration
{
    /// <summary>
    /// Configuration strategy for the internal timeout service.
    /// </summary>
    public class TimeoutServiceConfigurationStrategy : IServiceConfigurationStrategy
    {
        /// <summary>
        /// (Read-Write). The current component container instance for resolving or adding any resources.
        /// </summary>
        public IObjectBuilder ObjectBuilder
        {
            get;
            set;
        }

        /// <summary>
        /// (Read-Write). The current context under which the application has resources allocated to.
        /// </summary>
        public IMessageBus Bus
        {
            get; set;
        }

        /// <summary>
        /// This will configure the service based on the conventions needed and register it with the 
        /// <seealso cref="Carbon.ESB.Services.Registry.IBackgroundServiceRegistry">background service registry</seealso>to run 
        /// as an ancillary service to the framework.
        /// </summary>
        public ContextBackgroundService Configure()
        {
            // register the background service to respond to timeout messages and store in the background service registry:
            //var service = new TimeoutBackgroundService();
 
            // initialize any settings on the service and register instance:
            this.ObjectBuilder.Register(typeof(ITimeoutBackgroundService).Name, 
                                        typeof(ITimeoutBackgroundService), typeof(TimeoutBackgroundService));

            //return service;
            return null;
        }
    }
}