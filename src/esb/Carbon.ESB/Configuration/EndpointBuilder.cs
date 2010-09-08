using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Builder;
using Carbon.Core.Registries.For.ServiceEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.ESB.Registries.Endpoints;

namespace Carbon.ESB.Configuration
{
    public interface IEndpointBuilder
    {
        IEndpointBuilder Bind<TEndpoint>() where TEndpoint : class;
        IEndpointBuilder ToChannel(string channel);
        IEndpointBuilder ToUri(string uri);
    }

    public class EndpointBuilder : IEndpointBuilder
    {
        private readonly IObjectBuilder m_builder;
        private Type m_endpoint = null; 

        public EndpointBuilder(IObjectBuilder builder)
        {
            m_builder = builder;
        }

        public IEndpointBuilder Bind<TEndpoint>() where TEndpoint : class
        {
            return Bind(typeof(TEndpoint));
        }

        public IEndpointBuilder Bind(Type component)
        {
            m_endpoint = component;

            var activator = m_builder.Resolve<IMessageEndpointActivator>();
            return this;
        }

        public IEndpointBuilder ToChannel(string channel)
        {
            return this;
        }

        public IEndpointBuilder ToUri(string uri)
        {
            return this;
        }

    }
}
