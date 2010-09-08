using System;
using System.Configuration;
using Carbon.Channel.Registry;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Dispatcher;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Carbon.ESB.Pipeline;
using Carbon.ESB.Services;
using Carbon.ESB.Services.Registry;
using Castle.Core;
using Castle.MicroKernel.Facilities;
using Carbon.Core.Builder;
using Carbon.Windsor.Container.Integration;
using Carbon.ESB.Registries.Endpoints;
using Carbon.ESB.Internals;

namespace Carbon.ESB.Configuration
{
    public class CarbonEsbFacility : AbstractFacility
    {
        public const string FACILITY_ID = "carbon.esb";

        public void Initialize()
        {
            this.Init();
        }

        protected override void Init()
        {
            // add the native container abstraction to the underlying container for component resolution:
            Kernel.AddComponent(typeof(IObjectBuilder).Name, typeof(IObjectBuilder), 
                typeof(WindsorContainerObjectBuilder), LifestyleType.Singleton);

            #region -- register the core components in the container --

            Kernel.AddComponent(typeof(IReflection).Name, 
                typeof(IReflection),
                typeof(DefaultReflection), 
                LifestyleType.Transient);

            Kernel.AddComponent(typeof(IEndpointComponentScanner).Name, 
                typeof(IEndpointComponentScanner), 
                typeof(EndpointComponentScanner), 
                LifestyleType.Singleton);

            Kernel.AddComponent(typeof(IMessageDispatcher).Name, 
                typeof(IMessageDispatcher),
                typeof(MessageDispatcher));

            Kernel.AddComponent(typeof(ISubscriptionBuilder).Name, typeof(ISubscriptionBuilder),
                      typeof(SubscriptionBuilder));

            Kernel.AddComponent(typeof(IChannelRegistry).Name, typeof(IChannelRegistry),
                                typeof(ChannelRegistry), LifestyleType.Singleton);

            Kernel.AddComponent(typeof(ISerializationProvider).Name, typeof(ISerializationProvider),
                                typeof(DataContractSerializationProvider), LifestyleType.Singleton);

            Kernel.AddComponent(typeof(IServiceBusEndpointRegistry).Name, typeof(IServiceBusEndpointRegistry),
                                typeof(ServiceBusEndpointRegistry), LifestyleType.Singleton);

            Kernel.AddComponent(typeof(IAdapterRegistry).Name, typeof(IAdapterRegistry),
                                typeof(AdapterRegistry), LifestyleType.Singleton);

            Kernel.AddComponent(typeof(IAdapterFactory).Name, typeof(IAdapterFactory),
                                typeof(AdapterFactory));

            Kernel.AddComponent(typeof(IMessageEndpointActivator).Namespace,
                                typeof(IMessageEndpointActivator), typeof(MessageEndpointActivator));

            Kernel.AddComponent(typeof(IAdapterMessagingTemplate).Name, typeof(IAdapterMessagingTemplate),
                                typeof(AdapterMessagingTemplate));

            Kernel.AddComponent(typeof(IBackgroundServiceRegistry).Name, typeof(IBackgroundServiceRegistry),
                                typeof(BackgroundServiceRegistry), LifestyleType.Singleton);

            Kernel.AddComponent(typeof(IMessageBusMessagingPipeline).Name, 
                typeof(IMessageBusMessagingPipeline),
                typeof(MessageBusMessagingPipeline), 
                LifestyleType.Singleton);

            #endregion

            FindAndRegisterBackgroundServices();
            FindAndRegisterAllEndpoints();
            FindAndBootStrapAllEndpoints();

        }

        private void FindAndRegisterBackgroundServices()
        {
            var reflection = Kernel.Resolve<IReflection>();
            var services = reflection.FindConcreteTypesImplementingInterfaceAndBuild(typeof(IServiceConfigurationStrategy),
                                                                                    this.GetType().Assembly);
            foreach (var service in services)
            {
                var instance = service as IServiceConfigurationStrategy;
                if (instance != null)
                {
                    instance.ObjectBuilder = Kernel.Resolve<IObjectBuilder>();
                    instance.Configure();
                }
            }
        }

        private void FindAndRegisterAllEndpoints()
        {
            var reflection = Kernel.Resolve<IReflection>();

            #region --  inspect all of the elements in the configuration and build the concrete instance based on the definitions --
            var elementBuilderInstances =
                reflection.FindConcreteTypesImplementingInterfaceAndBuild(typeof(AbstractElementBuilder),
                                                                          this.GetType().Assembly);

            for (var index = 0; index < FacilityConfig.Children.Count; index++)
            {
                var element = FacilityConfig.Children[index];

                if (element == null)
                    continue;

                foreach (var elementBuilderInstance in elementBuilderInstances)
                {
                    var builder = elementBuilderInstance as AbstractElementBuilder;

                    if (builder.IsMatchFor(element.Name))
                    {
                        builder.Kernel = Kernel;
                        builder.Build(element);
                        break;
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// This will inspect the set of libraries in the executable directory for all instances 
        /// of <see cref="AbstractBootStrapper">a bootstrapper</see> and configure it based
        /// on user conventions.
        /// </summary>
        private void FindAndBootStrapAllEndpoints()
        {
            var reflection = Kernel.Resolve<IReflection>();

            var bootStrapperInstances =
                reflection.FindConcreteTypesImplementingInterfaceAndBuild(typeof(AbstractBootStrapper),
                                                                          this.GetType().Assembly);

            foreach (var instance in bootStrapperInstances)
            {
                var bootstrapper = instance as AbstractBootStrapper;

                try
                {
                    bootstrapper.Builder = Kernel.Resolve<IObjectBuilder>();
                    bootstrapper.Configure();
                }
                catch (Exception exception)
                {
                    var msg =
                        string.Format(
                            "An error has occurred while attempting to configure the bootstrapper '{0}': Reason: {1}. " +
                            "Please check the configuration of the bootstrapper for your custom component(s).",
                            bootstrapper.GetType().FullName, exception.Message);
                    throw new Exception(msg, exception);
                }
            }

        }
    }
}
