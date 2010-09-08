using System;
using System.Collections.Generic;
using Carbon.Channel.Registry;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Reflection;
using Carbon.Integration.Tests.Api.Surface;
using Carbon.Core;
using Carbon.Integration.Tests.Api.Surface.Ports;
using Carbon.Core.Adapter.Registry;

namespace Carbon.Integration.Tests.Api
{
    public interface IIntegrationContext
    {
        void Start();
        void Stop();

        void LoadSurface<TSurface>() where TSurface : AbstractIntegrationSurface;
        void LoadSurface(Type surface);

        void Send(string channel, IEnvelope envelope);
        void LoadAllSurfaces();
    }

    public class IntegrationContext : IIntegrationContext
    {
        private readonly IObjectBuilder m_object_builder;
        private readonly IAdapterRegistry m_adapter_registry;
        private readonly IChannelRegistry m_channel_registry;
        private List<AbstractIntegrationSurface> m_surfaces = null;

        public IntegrationContext(
            IObjectBuilder objectBuilder,
            IAdapterRegistry adapterRegistry,
            IChannelRegistry channelRegistry)
        {
            m_object_builder = objectBuilder;
            m_adapter_registry = adapterRegistry;
            m_channel_registry = channelRegistry;
            m_surfaces = new List<AbstractIntegrationSurface>();
        }

        public void Start()
        {
            this.ConfigureAllSurfaces();
            m_adapter_registry.Start();
        }

        public void Stop()
        {
            m_adapter_registry.Stop();
        }

        public void LoadSurface<TSurface>() 
            where TSurface : AbstractIntegrationSurface
        {
            this.LoadSurface(typeof(TSurface));
        }

        public void LoadSurface(Type surface)
        {
            try
            {
                m_object_builder.Register(surface.Name, surface, ActivationStyle.AsSingleton);
                var instance = m_object_builder.Resolve(surface);
                
                if(m_surfaces.Find( x => x.GetType() == instance.GetType() ) == null)
                    m_surfaces.Add(instance as AbstractIntegrationSurface);
            }
            catch (Exception exception)
            {
                throw;
            }
            
        }

        public void LoadAllSurfaces()
        {
            var scanner = m_object_builder.Resolve<IIntegrationSurfaceScanner>();
            scanner.Scan();

            foreach (var surface in scanner.Surfaces)
            this.LoadSurface(surface);
        }

        public void Send(string channel, IEnvelope envelope)
        {
            try
            {
                var targetedChanel = m_channel_registry.FindChannel(channel);
                targetedChanel.Send(envelope);
            }
            catch (Exception exception)
            {
                if (!OnIntegrationContextError(exception))
                    throw;
            }
        }

        private void ConfigureAllSurfaces()
        {
            foreach (var surface in m_surfaces)
            {
                var instance = m_object_builder.Resolve(surface.GetType()) as AbstractIntegrationSurface;
                instance.Configure();
           }
        }

        private bool OnIntegrationContextError(Exception exception)
        {
            return false;
        }

    }
}