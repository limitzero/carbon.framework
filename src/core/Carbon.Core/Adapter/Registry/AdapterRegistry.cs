using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Carbon.Core.Adapter.Impl.Null;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Registry
{
    public class AdapterRegistry : IAdapterRegistry
    {
        private static List<AbstractInputChannelAdapter> m_input_adapters = null;
        private static List<AbstractOutputChannelAdapter> m_output_adapters = null;

        private readonly IObjectBuilder m_builder;

        public IList<AbstractInputChannelAdapter> InputAdapters
        {
            get { return m_input_adapters; }
        }

        public IList<AbstractOutputChannelAdapter> OutputAdapters
        {
            get { return m_output_adapters; }
        }

        public bool IsRunning { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="builder"></param>
        //public AdapterRegistry(IReflection reflection)
        public AdapterRegistry(IObjectBuilder builder)
        {
            m_builder = builder;

            if (m_input_adapters == null)
                m_input_adapters = new List<AbstractInputChannelAdapter>();

            if (m_output_adapters == null)
                m_output_adapters = new List<AbstractOutputChannelAdapter>();
        }

        public void Dispose()
        {
            this.Stop();
        }

        public void Start()
        {
            if (IsRunning)
                return;

            if (m_input_adapters != null)
                foreach (var adapter in m_input_adapters)
                {
                    adapter.AdapterStarted += AdapterStarted;
                    adapter.AdapterStopped += AdapterStopped;
                    adapter.AdapterError += AdapterError;
                    adapter.Start();
                }

            if (m_output_adapters != null)
                foreach (var adapter in m_output_adapters)
                {
                    adapter.AdapterStarted += AdapterStarted;
                    adapter.AdapterStopped += AdapterStopped;
                    adapter.AdapterError += AdapterError;
                    adapter.Start();
                }

            IsRunning = true;
        }

        public void Stop()
        {
            if (m_input_adapters != null)
                foreach (var adapter in m_input_adapters)
                {
                    adapter.Stop();
                    adapter.AdapterStarted -= AdapterStarted;
                    adapter.AdapterStopped -= AdapterStopped;
                    adapter.AdapterError -= AdapterError;
                }

            if (m_output_adapters != null)
                foreach (var adapter in m_output_adapters)
                {
                    adapter.Stop();
                    adapter.AdapterStarted -= AdapterStarted;
                    adapter.AdapterStopped -= AdapterStopped;
                    adapter.AdapterError -= AdapterError;
                }

            IsRunning = false;
        }

        /// <summary>
        /// This will scan an assebly by name for all adapters that can be registered for use.
        /// </summary>
        /// <param name="asssemblyName">Name of the assembly to scan.</param>
        public void Scan(string asssemblyName)
        {
            this.Scan(Assembly.Load(asssemblyName));
        }

        /// <summary>
        /// This will scan an assembly for all adapters that can be registered for use.
        /// </summary>
        /// <param name="assembly">Assembly to scan</param>
        public void Scan(Assembly assembly)
        {
            var reflection = m_builder.Resolve<IReflection>();

            var scannedAdapterConfigurations =
                reflection.FindConcreteTypesImplementingInterfaceAndBuild(typeof(IAdapterRegistrationStrategy),
                                                                          assembly);
            foreach (var scannedAdapterConfiguration in scannedAdapterConfigurations)
            {
                try
                {
                    if (scannedAdapterConfiguration == null) continue;

                    var configuration = scannedAdapterConfiguration as IAdapterRegistrationStrategy;
                    configuration.ObjectBuilder = m_builder;
                    var adapterConfiguration = configuration.Configure();

                    if (adapterConfiguration == null) continue;

                    try
                    {
                        var message =
                            string.Format("Adapter configuration registered for scheme '{0}' with addressing uri '{1}'.",
                                          adapterConfiguration.Scheme, adapterConfiguration.Uri);
                        var logUri = new Uri(Constants.LogUris.INFO_LOG_URI);
                        this.m_builder.Resolve<IAdapterMessagingTemplate>().DoSend(logUri, new Envelope(message));
                    }
                    catch
                    {
                    }

                    this.RegisterAdapterConfiguration(adapterConfiguration);
                }
                catch (Exception exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// This will register the adapter configuration in the registry for later 
        /// access on building.
        /// </summary>
        /// <param name="configuration"></param>
        public void RegisterAdapterConfiguration(IAdapterConfiguration configuration)
        {
            // register the input and output adapter in the container for 
            // further use by the framework for auto-creating adapters via 
            // the component container for the adapter factory:
            var inputAdapterId = string.Concat(configuration.Scheme, Constants.INPUT_ADAPTER_SUFFIX);
            var outputAdapterId = string.Concat(configuration.Scheme, Constants.OUTPUT_ADAPTER_SUFFIX);

            if (configuration.InputChannelAdapter == null)
                m_builder.Register(inputAdapterId, typeof(NullInputChannelAdapter));
            else
            {
                m_builder.Register(inputAdapterId, configuration.InputChannelAdapter);
            }

            if (configuration.OutputChannelAdapter == null)
                m_builder.Register(outputAdapterId, typeof(NullOutputChannelAdapter));
            else
            {
                m_builder.Register(outputAdapterId, configuration.OutputChannelAdapter);
            }
        }

        /// <summary>
        /// This will inspect the uri and based on the schemes for adapters 
        /// that are registered, create the input adapter instance.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>
        /// The type of the concrete instance of the input adapter defined by the uri scheme.
        /// </returns>
        public AbstractInputChannelAdapter FindInputAdapterFromUriScheme(string uri)
        {
            AbstractInputChannelAdapter adapter = new NullInputChannelAdapter();

            try
            {
                var key = string.Concat(new Uri(uri).Scheme, Constants.INPUT_ADAPTER_SUFFIX);
                adapter = m_builder.Resolve(key) as AbstractInputChannelAdapter;
            }
            catch
            {

            }

            return adapter;
        }


        /// <summary>
        /// This will inspect the uri and based on the schemes for adapters 
        /// that are registered, create the output adapter instance.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>
        /// The type of the concrete instance of the output adapter defined by the uri scheme.
        /// </returns>
        public AbstractOutputChannelAdapter FindOutputAdapterFromUriScheme(string uri)
        {
            AbstractOutputChannelAdapter adapter = new NullOutputChannelAdapter();

            try
            {
                var key = string.Concat(new Uri(uri).Scheme, Constants.OUTPUT_ADAPTER_SUFFIX);
                adapter = m_builder.Resolve(key) as AbstractOutputChannelAdapter;
            }
            catch
            {
                
            }

            return adapter;
        }

        /// <summary>
        /// This will register a pre-build input channel adapter configuration.
        /// </summary>
        /// <param name="adapter"></param>
        public void RegisterInputChannelAdapter(AbstractInputChannelAdapter adapter)
        {

            try
            {
                var key = string.Concat(adapter.GetScheme(), Constants.INPUT_ADAPTER_SUFFIX);
                m_builder.Register(key, adapter.GetType());

                if (!m_input_adapters.Contains(adapter))
                    m_input_adapters.Add(adapter);
            }
            catch
            {
                // adapter implementation already registered:
            }
        }

        /// <summary>
        /// This will register a pre-build output channel adapter configuration.
        /// </summary>
        /// <param name="adapter"></param>
        public void RegisterOutputChannelAdapter(AbstractOutputChannelAdapter adapter)
        {
            try
            {
                var key = string.Concat(adapter.GetScheme(), Constants.OUTPUT_ADAPTER_SUFFIX);
                m_builder.Register(key, adapter.GetType());

                if (!m_output_adapters.Contains(adapter))
                    m_output_adapters.Add(adapter);
            }
            catch
            {
                // adapter implementation already registered:
            }
        }

        private void AdapterError(object sender, ChannelAdapterErrorEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI), new Envelope(e.Message + " " + e.Exception.ToString()));
        }

        private void AdapterStopped(object sender, ChannelAdapterStoppedEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI), new Envelope(e.Message));
        }

        private void AdapterStarted(object sender, ChannelAdapterStartedEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI), new Envelope(e.Message));
        }

        ~AdapterRegistry()
        {
            if (m_input_adapters != null)
            {
                m_input_adapters.Clear();
                m_input_adapters = null;
            }

            if (m_output_adapters != null)
            {
                m_output_adapters.Clear();
                m_output_adapters = null;
            }
        }
    }
}