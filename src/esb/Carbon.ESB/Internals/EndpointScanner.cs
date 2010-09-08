using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Carbon.Core.Builder;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;

namespace Carbon.ESB.Internals
{
    public class EndpointComponentScanner : IEndpointComponentScanner
    {
        private readonly IObjectBuilder m_builder;
        private List<Type> m_components = null;

        public ReadOnlyCollection<Type> Components { get; private set; }

        public EndpointComponentScanner(IObjectBuilder builder)
        {
            m_builder = builder;

            if (m_components == null)
                m_components = new List<Type>();
        }

        public void ScanLocation(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFile(file);
                    this.Scan(asm);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Scan(params string[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    var asm = Assembly.Load(assembly);
                    this.Scan(asm);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public void Scan(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract)
                        RegisterEndpointInContainer(type);
                }
            }
        }

        private void RegisterEndpointInContainer(Type endpoint)
        {
            if (endpoint.GetCustomAttributes(typeof(MessageEndpointAttribute), true).Length == 0) return;

            try
            {
                m_builder.Register(endpoint.Name, endpoint);
                m_components.Add(endpoint);
                this.Components = m_components.AsReadOnly();
            }
            catch
            {
                // must already be in the container...ignore:
            }

        }
    }
}