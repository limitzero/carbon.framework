using System;
using System.Collections.Generic;
using System.IO;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Dispatcher;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Carbon.Windsor.Container.Integration;
using Castle.Core;
using Castle.MicroKernel.Facilities;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl;
using Carbon.Core.Registries.For.MessageEndpoints;
using System.Reflection;
using Carbon.Integration.Pipeline;
using Carbon.Integration.Scheduler;
using Carbon.Integration.Dsl.Surface.Registry;
using Carbon.Core.Pipeline;
using Carbon.Core.Pipeline.Component;
using Carbon.Integration.Stereotypes.Gateway.Impl;
using Castle.MicroKernel;
using Carbon.Core.Channel.Template;

namespace Carbon.Integration.Configuration
{
    public class CarbonIntegrationFacility : AbstractFacility
    {
        public const string FACILITY_ID = "carbon.integration";

        public void Initialize()
        {
            this.Init();
        }

        protected override void Init()
        {
            // add the native container abstraction to the underlying container for component resolution:
            Kernel.AddComponent(typeof(IObjectBuilder).Name,
                typeof(IObjectBuilder),
                typeof(WindsorContainerObjectBuilder),
                LifestyleType.Singleton);

            var objectBuilder = Kernel.Resolve<IObjectBuilder>(); 
            objectBuilder.SetContainer(Kernel);

            #region -- register the core components in the container --

            objectBuilder.Register(typeof(IReflection).Name,
                typeof(IReflection),
                typeof(DefaultReflection),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IIntegrationContext).Name,
                typeof(IIntegrationContext),
                typeof(IntegrationContext),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IIntegrationMessagingPipeline).Name,
                typeof(IIntegrationMessagingPipeline),
                typeof(IntegrationMessagingPipeline),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IGatewayMessageForwarder).Name,
                typeof(IGatewayMessageForwarder),
                typeof(GatewayMessageForwarder),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IIntegrationSurfaceScanner).Name,
                typeof(IIntegrationSurfaceScanner),
                typeof(IntegrationSurfaceScanner),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IMessageDispatcher).Name,
                typeof(IMessageDispatcher),
                typeof(MessageDispatcher),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(ISubscriptionBuilder).Name,
                typeof(ISubscriptionBuilder),
                typeof(SubscriptionBuilder),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IChannelRegistry).Name,
                typeof(IChannelRegistry),
                typeof(ChannelRegistry),
                ActivationStyle.AsSingleton);

            objectBuilder.Register(typeof(ISerializationProvider).Name,
                typeof(ISerializationProvider),
                typeof(DataContractSerializationProvider),
                ActivationStyle.AsSingleton);

            objectBuilder.Register(typeof(IMessageEndpointRegistry).Name,
                typeof(IMessageEndpointRegistry),
                typeof(MessageEndpointRegistry),
                ActivationStyle.AsSingleton);

            objectBuilder.Register(typeof(IAdapterRegistry).Name,
                typeof(IAdapterRegistry),
                typeof(AdapterRegistry),
                ActivationStyle.AsSingleton);

            objectBuilder.Register(typeof(IAdapterFactory).Name,
                typeof(IAdapterFactory),
                typeof(AdapterFactory),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IMessageEndpointActivator).Namespace,
                typeof(IMessageEndpointActivator),
                typeof(MessageEndpointActivator),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IAdapterMessagingTemplate).Name,
                typeof(IAdapterMessagingTemplate),
                typeof(AdapterMessagingTemplate),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IChannelMessagingTemplate).Name,
                typeof(IChannelMessagingTemplate),
                typeof(ChannelMessagingTemplate),
                ActivationStyle.AsInstance);

            objectBuilder.Register(typeof(IScheduler).Name,
                typeof(IScheduler),
                typeof(Scheduler.Scheduler),
                ActivationStyle.AsSingleton);

            objectBuilder.Register(typeof(ISurfaceRegistry).Name,
                typeof(ISurfaceRegistry),
                typeof(SurfaceRegistry),
                ActivationStyle.AsSingleton);

            #endregion

            // scan all of the neccessary items to automatically
            // create the elements based on the attribute-denoted components:
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            LoadAllAdapters(files);

            LoadMessagesForSerializationEngine(files);

            LoadAllPipelineComponents(files);

            LoadOnDemandPipelineConfigurations(files);

            LoadItemsForScheduler(files);

            // this must be done last:
            LoadItemsFromConfiguration();

            //Kernel.Resolve<IObjectBuilder>().Register(typeof(IKernel).Name, typeof(IKernel), Kernel);
        }

        private void LoadAllAdapters(IEnumerable<string> files)
        {
            // custom and internally provided channel adapters:
            var adapterRegistry = Kernel.Resolve<IAdapterRegistry>();
            adapterRegistry.Scan(this.GetType().Assembly);

            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    if (assembly == null) continue;
                    adapterRegistry.Scan(assembly);
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// This will search for all components marked with the [Message] annotation 
        /// and load them into the serialization engine.
        /// </summary>
        /// <param name="files"></param>
        private void LoadMessagesForSerializationEngine(IEnumerable<string> files)
        {
            // message serialization:
            var serializer = Kernel.Resolve<ISerializationProvider>();
            serializer.Scan(this.GetType().Assembly);

            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    if (assembly == null) continue;
                    serializer.Scan(assembly);
                }
                catch
                {
                    continue;
                }
            }
        }

        private void LoadItemsForScheduler(IEnumerable<string> files)
        {
            var scheduler = Kernel.Resolve<IScheduler>();

            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    if (assembly == null) continue;
                    scheduler.ScanAndRegister(new List<Type>(assembly.GetTypes()));
                }
                catch
                {
                    continue;
                }
            }
        }

        private void LoadAllPipelineComponents(IEnumerable<string> files)
        {
            var reflection = Kernel.Resolve<IReflection>();


            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);

                    var pipelineConfigurations =
                        reflection.FindConcreteTypesImplementingInterface(typeof(AbstractPipelineComponent),
                                                                       assembly);

                    foreach (var configuration in pipelineConfigurations)
                    {
                        try
                        {
                            Kernel.AddComponent(configuration.Name, configuration);
                            var template = Kernel.Resolve<IAdapterMessagingTemplate>();

                            if (template != null)
                                template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                                new Envelope(string.Format("Pipeline component '{0}' loaded.",
                                                                           configuration.FullName)));

                        }
                        catch (Exception exception)
                        {
                            // pipeline component already loaded;
                            continue;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private void LoadOnDemandPipelineConfigurations(IEnumerable<string> files)
        {
            var reflection = Kernel.Resolve<IReflection>();


            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);

                    var pipelineConfigurations =
                        reflection.FindConcreteTypesImplementingInterface(typeof(IOnDemandPipelineConfiguration),
                                                                       assembly);

                    foreach (var configuration in pipelineConfigurations)
                    {
                        try
                        {
                            Kernel.AddComponent(configuration.Name, configuration);
                            var template = Kernel.Resolve<IAdapterMessagingTemplate>();

                            if (template != null)
                                template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                                new Envelope(string.Format("On-demand pipeline configuration '{0}' loaded.",
                                                                           configuration.FullName)));

                        }
                        catch (Exception exception)
                        {
                            // on-demand pipeline configuration already loaded:
                            continue;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

        }

        private void LoadItemsFromConfiguration()
        {
            var reflection = Kernel.Resolve<IReflection>();

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
        }

    }
}