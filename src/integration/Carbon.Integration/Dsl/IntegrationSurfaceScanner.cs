using System;
using System.IO;
using System.Reflection;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Reflection;
using Carbon.Integration.Dsl.Surface;
using Carbon.Integration.Dsl.Surface.Registry;

namespace Carbon.Integration.Dsl
{
    /// <summary>
    /// Class for inspecting all libraries for integration surface components and registering them for use.
    /// </summary>
    public class IntegrationSurfaceScanner : IIntegrationSurfaceScanner
    {
        private readonly IObjectBuilder m_builder;
        private readonly IReflection m_reflection;
        private readonly ISurfaceRegistry m_surface_registry;

        public IntegrationSurfaceScanner(
            IObjectBuilder builder,
            IReflection reflection, 
            ISurfaceRegistry registry)
        {
            m_builder = builder;
            m_reflection = reflection;
            m_surface_registry = registry;
        }

        public void Scan()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            foreach (var file in files)
            {
               
                try
                {
                    var asm = Assembly.LoadFile(file);
                    var types = m_reflection.FindConcreteTypesImplementingInterface(typeof(AbstractIntegrationComponentSurface),
                                                                                    asm);
                    foreach (var type in types)
                    {
                        try
                        {
                            m_builder.Register(type.Name, type);
                            var instance = m_builder.Resolve(type) as AbstractIntegrationComponentSurface;
                            m_surface_registry.Register(instance);
                        }
                        catch (Exception exception)
                        {
                            // instance already registered.
                        }
                    }
                }
                catch (Exception exception)
                {
                    // could not load file...
                    continue;
                }
               
            }
        }

    }
}