using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Carbon.Integration.Dsl.Surface.Registry
{
    public class SurfaceRegistry : ISurfaceRegistry
    {
        private static object m_lock = new object();
        private static List<AbstractIntegrationComponentSurface> m_surfaces = null;

        public SurfaceRegistry()
        {
            if (m_surfaces == null)
                m_surfaces = new List<AbstractIntegrationComponentSurface>();
            this.Surfaces = m_surfaces.AsReadOnly();
        }

        public ReadOnlyCollection<AbstractIntegrationComponentSurface> Surfaces
        {
            get;
            private set;
        }

        public void Register(AbstractIntegrationComponentSurface surface)
        {
            if (m_surfaces.Contains(surface)) return;

            lock (m_lock)
            {
                m_surfaces.Add(surface);
                this.Surfaces = m_surfaces.AsReadOnly();
            }

        }

        public TComponentSurface Find<TComponentSurface>() where TComponentSurface : AbstractIntegrationComponentSurface
        {
            var retval = default(TComponentSurface);

            foreach (var surface in Surfaces)
            {
                if(surface.GetType() != typeof(TComponentSurface))   continue;
                retval = (TComponentSurface)surface;
                break;
            }

            return retval;

        }

        ~SurfaceRegistry()
        {
            if (m_surfaces != null)
            {
                m_surfaces.Clear();
                m_surfaces = null;
            }
        }
    }
}