using System;
using System.Collections.Generic;
using System.IO;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.ESB.Internals;
using Carbon.ESB.Registries.Endpoints;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Subscriptions.Persister;
using Castle.Core;
using Castle.Core.Configuration;
using System.Reflection;
using Carbon.Core.Builder;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Null;
using Carbon.ESB.Services.Impl.Timeout.Persister;

namespace Carbon.ESB.Configuration
{
    public class MessageBusElementConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "message-bus";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            // add the message bus implementation:
            Kernel.AddComponent(typeof(IMessageBus).Name, typeof(IMessageBus),
                                typeof(MessageBus), LifestyleType.Singleton);

            #region -- configuration specific information for bus --
            var annotationDriven = configuration.Attributes["annotation-driven"];
            var localChannel = configuration.Attributes["local-channel"];
            var localAddress = configuration.Attributes["local-address"];
            var subscriptionAddress = configuration.Attributes["subscription-address"];
            var concurrency = configuration.Attributes["concurrency"];
            var frequency = configuration.Attributes["frequency"];
            #endregion

            // scan all of the neccessary items to automatically
            // create the elements based on the attribute-denoted components:
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            #region -- initialize components in the container needed for the message bus --

            // custom and internally provided channel adapters:
            //var adapterRegistry = Kernel.Resolve<IAdapterRegistry>();
            //adapterRegistry.Scan(this.GetType().Assembly);

            //foreach (var file in files)
            //{
            //    try
            //    {
            //        var assembly = this.LoadAssemblyFromFile(file);
            //        if (assembly == null) continue;
            //        adapterRegistry.Scan(assembly);
            //    }
            //    catch
            //    {
            //        continue;
            //    }
            //}

            // internally designed services for the message bus:
            // Kernel.Resolve<IBackgroundServiceRegistry>().Scan(this.GetType().Assembly);

            #endregion

            LoadAllAdapters(files);

            LoadMessagesForSerializationEngine(files);

            FindAndLoadEndpoints(files);

            BuildEndpointActivatorForEndpoints();

            BuildAllSubscriptions(files);

            ConfigureOnDemandComponents(files);

            ConfigureDefaultImplementations();

            BootStrapFramework(files);

            var serializer = Kernel.Resolve<ISerializationProvider>();
            serializer.Initialize();

            #region -- configure the bus --
            var bus = Kernel.Resolve<IMessageBus>();

            var value = false;

            if (bool.TryParse(annotationDriven, out value))
                bus.IsAnnotationDriven = value;
            else
            {
                bus.IsAnnotationDriven = false;
            }

            if (string.IsNullOrEmpty(localChannel) || string.IsNullOrEmpty(localAddress)) return;

            bus.LocalChannel = localChannel;
            bus.LocalAddress = localAddress;
            bus.SubscriptionAddress = subscriptionAddress;

            // set the adapter for the bus to receive messages:
            var intValue = 0;
            var inputAdapter = Kernel.Resolve<IAdapterFactory>().BuildInputAdapterFromUri(bus.LocalChannel,
                                                                                          bus.LocalAddress);

            if (!string.IsNullOrEmpty(concurrency))
                if (Int32.TryParse(concurrency, out intValue))
                    inputAdapter.Concurrency = intValue;
                else
                {
                    inputAdapter.Concurrency = 1;
                }

            if (!string.IsNullOrEmpty(frequency))
                if (Int32.TryParse(frequency, out intValue))
                    inputAdapter.Frequency = Convert.ToInt32(intValue);
                else
                {
                    inputAdapter.Frequency = 1;
                }

            bus.Endpoint = inputAdapter;

            // register the bus endpoint:
            var activator = Kernel.Resolve<IMessageEndpointActivator>();
            activator.ActivationStyle = EndpointActivationStyle.ActivateOnMessageReceived;
            activator.SetInputChannel(bus.LocalChannel);
            activator.SetEndpointInstance(bus);
            Kernel.Resolve<IServiceBusEndpointRegistry>().Register(activator); // this will pick up the Publish(IEnvelope message) method!!!
            #endregion

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
                    var assembly = this.LoadAssemblyFromFile(file);
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
                    var assembly = this.LoadAssemblyFromFile(file);
                    if (assembly == null) continue;
                    serializer.Scan(assembly);
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        ///  Create the list of endpoints in the container based on the 
        ///  search that is conducted for each assembly in the executable
        ///  directory.
        /// </summary>
        /// <param name="files">List of assemblies to scan for endpoint implementations.</param>
        private void FindAndLoadEndpoints(IEnumerable<string> files)
        {
            var endpointScanner = Kernel.Resolve<IEndpointComponentScanner>();

            foreach (var file in files)
            {
                try
                {
                    var asm = LoadAssemblyFromFile(file);
                    endpointScanner.Scan(asm);
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// This will build a custom end point activator for each endpoint
        /// and register the endpoint in the container.
        /// </summary>
        private void BuildEndpointActivatorForEndpoints()
        {
            var endpointScanner = Kernel.Resolve<IEndpointComponentScanner>();
            var serializer = Kernel.Resolve<ISerializationProvider>();
            var channelRegistry = Kernel.Resolve<IChannelRegistry>();
            var servicebusEndpointRegistry = Kernel.Resolve<IServiceBusEndpointRegistry>();

            foreach (var component in endpointScanner.Components)
            {
                var channel = this.ExtractInputChannel(component);

                if (channelRegistry.FindChannel(channel) is NullChannel)
                    channelRegistry.RegisterChannel(channel);

                var activator = Kernel.Resolve<IMessageEndpointActivator>();
                activator.ActivationStyle = EndpointActivationStyle.ActivateOnMessageReceived;
                activator.SetEndpointInstance(Kernel.Resolve(component));
                activator.SetInputChannel(channel);

                servicebusEndpointRegistry.Register(activator);
                serializer.AddCustomType(component);
            }
        }

        /// <summary>
        ///  This will configure all of the subscriptions for the bus.
        /// </summary>
        /// <param name="files"></param>
        private void BuildAllSubscriptions(IEnumerable<string> files)
        {
            // subscriptions (local):
            ISubscriptionPersister subscriptionRegistry = null;

            try
            {
                subscriptionRegistry = Kernel.Resolve<ISubscriptionPersister>();
            }
            catch
            {
                Kernel.Resolve<IObjectBuilder>().Register(typeof(ISubscriptionPersister).Name,
                                                         typeof(ISubscriptionPersister),
                                                         typeof(InMemorySubscriptionPersister),
                                                         ActivationStyle.AsSingleton);
                subscriptionRegistry = Kernel.Resolve<ISubscriptionPersister>();
            }

            subscriptionRegistry.Scan(this.GetType().Assembly);

            // subscriptions (external):
            foreach (var file in files)
            {
                try
                {
                    var asm = LoadAssemblyFromFile(file);
                    subscriptionRegistry.Scan(asm);
                }
                catch
                {
                    continue;
                }
            }

        }

        /// <summary>
        /// This will invoke any custom bootstrap routines for custom 
        /// configuration.
        /// </summary>
        /// <param name="files"></param>
        private void BootStrapFramework(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                try
                {
                    var assembly = this.LoadAssemblyFromFile(file);

                    var bootstrappers = Kernel.Resolve<IReflection>()
                        .FindConcreteTypesImplementingInterfaceAndBuild(typeof(AbstractBootStrapper), assembly);

                    foreach (var bootstrapper in bootstrappers)
                    {
                        var bscfg = bootstrapper as AbstractBootStrapper;

                        foreach (var type in assembly.GetTypes())
                        {
                            if (!bscfg.IsMatchFor(type)) continue;

                            bscfg.Builder = Kernel.Resolve<IObjectBuilder>();
                            bscfg.Configure();
                            break;
                        }
                    }

                }
                catch
                {
                    continue;
                }
            }
        }

        private void ConfigureOnDemandComponents(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                try
                {
                    var assembly = this.LoadAssemblyFromFile(file);

                    var componentRegistrations = Kernel.Resolve<IReflection>()
                        .FindConcreteTypesImplementingInterfaceAndBuild(typeof(AbstractOnDemandComponentRegistration), assembly);

                    foreach (var registration in componentRegistrations)
                    {
                        var reg = registration as AbstractOnDemandComponentRegistration;

                        foreach (var type in assembly.GetTypes())
                        {
                            reg.Builder = Kernel.Resolve<IObjectBuilder>();
                            reg.Register();
                            break;
                        }
                    }

                }
                catch
                {
                    continue;
                }
            }
        }

        private void ConfigureDefaultImplementations()
        {
            #region -- saga persistance --
            // TODO: Think about this section of code and what you are trying to do with persistance for sagas...
            //try
            //{
            //    var persisterType = this.Kernel.Resolve<IReflection>().GetGenericVersionOf(typeof(ISagaPersister<>),
            //                                                                               new Type[] { typeof(object) });
            //    var sagaPersister = this.Kernel.Resolve(persisterType);
            //}
            //catch (Exception exception)
            //{
            //    this.Kernel.AddComponent(typeof(ISagaPersister<>).Name,
            //        typeof(ISagaPersister<>),
            //        typeof(InMemorySagaPersister<>),
            //        LifestyleType.Singleton);
            //}
            #endregion

            #region -- timeouts persistance --
            try
            {
                var timeoutsPersister = this.Kernel.Resolve<ITimeoutsPersister>();
            }
            catch (Exception exception)
            {
                this.Kernel.AddComponent(typeof(ITimeoutsPersister).Name,
                    typeof(ITimeoutsPersister),
                     typeof(InMemoryTimeoutsPersister),
                     LifestyleType.Singleton);
            }
            #endregion

            #region -- subscriptions persistance --
            try
            {
                var subscriptionPersister = this.Kernel.Resolve<ISubscriptionPersister>();
            }
            catch (Exception exception)
            {
                this.Kernel.AddComponent(typeof(ISubscriptionPersister).Name,
                     typeof(ISubscriptionPersister),
                     typeof(InMemorySubscriptionPersister),
                     LifestyleType.Singleton);
            }
            #endregion
        }

        /// <summary>
        /// This will retrieve the input channel for an endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private string ExtractInputChannel(Type endpoint)
        {
            var channel = string.Empty;
            var attributes = endpoint.GetCustomAttributes(typeof(MessageEndpointAttribute), true);
            if (attributes.Length > 0)
                channel = ((MessageEndpointAttribute)attributes[0]).InputChannel;
            return channel;
        }

        private Assembly LoadAssemblyFromFile(string file)
        {
            Assembly asm = null;

            try
            {
                asm = Assembly.LoadFile(file);
            }
            catch
            {

            }

            return asm;
        }
    }
}