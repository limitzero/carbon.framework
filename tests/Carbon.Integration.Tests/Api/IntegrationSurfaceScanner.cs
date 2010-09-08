using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Carbon.Core.Internals.Reflection;
using Carbon.Integration.Tests.Api.Surface;

namespace Carbon.Integration.Tests.Api
{
    public class IntegrationSurfaceScanner : IIntegrationSurfaceScanner
    {
        private readonly IReflection m_reflection;
        private List<Type> m_surfaces = null; 

        public ReadOnlyCollection<Type> Surfaces { get; private set; }

        public IntegrationSurfaceScanner(IReflection reflection)
        {
            m_reflection = reflection;
            m_surfaces = new List<Type>();
        }

        public void Scan()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            foreach (var file in files)
            {
                var asm = Assembly.LoadFile(file);
                var types = m_reflection.FindConcreteTypesImplementingInterface(typeof (AbstractIntegrationSurface),
                                                                                asm);
                foreach (var type in types)
                {
                    if(m_surfaces.Find( x => x.GetType() == type ) == null)
                        m_surfaces.Add(type);
                }
               
            }
        }

    }
}