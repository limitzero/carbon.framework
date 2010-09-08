using System;
using Carbon.ESB.Configuration;
using Carbon.ESB.Services.Registry;

namespace Carbon.ESB.Services.Impl.Timeout.Configuration
{
    /// <summary>
    /// Bootstrapper for the timeout service.
    /// </summary>
    public class TimeoutServiceBootstrapper : AbstractBootStrapper
    {
        public override bool IsMatchFor(Type component)
        {
            return component == typeof (TimeoutBackgroundService);
        }

        public override void Configure()
        {
            // (1) register the component in the container:
            this.Builder.Register(typeof(ITimeoutBackgroundService).Name,
                                  typeof(ITimeoutBackgroundService), 
                                  typeof(TimeoutBackgroundService));

            // (2) add the service to the service registry for the message bus:
            var service = this.Builder.Resolve<ITimeoutBackgroundService>();
            this.Builder.Resolve<IBackgroundServiceRegistry>().Register(service as ContextBackgroundService);
        }
    }
}