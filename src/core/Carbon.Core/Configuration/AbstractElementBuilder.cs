using System;
using Carbon.Channel.Registry;
using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.RuntimeServices;
using Castle.Core.Configuration;
using Castle.MicroKernel;

namespace Carbon.Core.Configuration
{
    public abstract class AbstractElementBuilder
    {
        public IKernel Kernel { get; set; }

        /// <summary>
        /// This will inspect the configuration element by 
        /// name to see if the concrete element builder 
        /// can build the element based on configuration.
        /// </summary>
        /// <param name="name">Name of the configuration element.</param>
        /// <returns></returns>
        public abstract bool IsMatchFor(string name);

        /// <summary>
        /// This will build the element based on the configuration
        /// definition and store it to the model repository.
        /// </summary>
        /// <param name="configuration"></param>
        public abstract void Build(IConfiguration configuration);

        /// <summary>
        /// This will register the source channel adapter to the adapters registry.
        /// </summary>
        /// <param name="adapter">Adapter to register</param>
        public void RegisterSourceChannelAdapter(AbstractInputChannelAdapter adapter)
        {
            Kernel.Resolve<IAdapterRegistry>().RegisterInputChannelAdapter(adapter);
        }

        /// <summary>
        /// This will register the target channel adapter to the adapters registry.
        /// </summary>
        /// <param name="adapter">Adapter to register</param>
        public void RegisterTargetChannelAdapter(AbstractOutputChannelAdapter adapter)
        {
            Kernel.Resolve<IAdapterRegistry>().RegisterOutputChannelAdapter(adapter);
        }

        /// <summary>
        /// This will register the input and/or output channels that are defined 
        /// for an adapter.
        /// </summary>
        /// <param name="inputChannel">Name of the input channel</param>
        /// <param name="outputChannel">Name of the output channel</param>
        public void RegisterChannels(string inputChannel, string outputChannel)
        {
            if (!string.IsNullOrEmpty(inputChannel))
            {
                var channel = this.Kernel.Resolve<IChannelRegistry>().FindChannel(inputChannel);
                if (channel is NullChannel)
                    this.Kernel.Resolve<IChannelRegistry>().RegisterChannel(inputChannel);
            }

            if (!string.IsNullOrEmpty(outputChannel))
            {
                var channel = this.Kernel.Resolve<IChannelRegistry>().FindChannel(outputChannel);
                if (channel is NullChannel)
                    this.Kernel.Resolve<IChannelRegistry>().RegisterChannel(outputChannel);
            }
        }

        /// <summary>
        /// Configures the basic polling and/or scheduling semantics for the input channel adapter.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="adapter"></param>
        public  void ConfigureInputChannelAdapterBasicStrategies(IConfiguration configuration, 
                                                                 AbstractInputChannelAdapter adapter) 
        {
            // read the strategies:
            var strategies = configuration.Children["strategies"];
            if (strategies != null)
            {
                // read the polling strategy:
                ConfigurePollingStrategy(strategies, adapter);

                // read the scheduling strategy:
                ConfigureSchedulingStrategy(strategies, adapter);
            }
        }

        /// <summary>
        /// Configures the basic polling and/or scheduling semantics for the output channel adapter.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="adapter"></param>
        public void ConfigureOutputChannelAdapterBasicStrategies(IConfiguration configuration,
                                                                 AbstractOutputChannelAdapter adapter)
        {
            // read the strategies:
            var strategies = configuration.Children["strategies"];
            if (strategies != null)
            {
                // read the polling strategy:
                ConfigurePollingStrategy(strategies, adapter);

                // read the scheduling strategy:
                ConfigureSchedulingStrategy(strategies, adapter);
            }
        }

        private static void ConfigurePollingStrategy(IConfiguration configuration, 
                                                     IBackgroundService adapter)
        {
            var pollingStrategy = configuration.Children["polling"];

            if (pollingStrategy != null)
            {
                var concurrency = Convert.ToInt32(pollingStrategy.Attributes["concurrency"]);
                var frequency = Convert.ToInt32(pollingStrategy.Attributes["frequency"]);
                adapter.Concurrency = concurrency;
                adapter.Frequency = frequency;
            }
        }

        private static void ConfigureSchedulingStrategy(IConfiguration configuration,
                                                        IBackgroundService adapter)
        {
            var scheduleStrategy = configuration.Children["scheduling"];

            if (scheduleStrategy != null)
            {
                var interval = Convert.ToInt32(scheduleStrategy.Attributes["interval"]);
                adapter.Interval =  interval;
            }
        }
    }
}