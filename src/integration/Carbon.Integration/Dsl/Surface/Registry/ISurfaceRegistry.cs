using System;
using System.Collections.ObjectModel;

namespace Carbon.Integration.Dsl.Surface.Registry
{
    public interface ISurfaceRegistry
    {
        ReadOnlyCollection<AbstractIntegrationComponentSurface> Surfaces { get;}
        void Register(AbstractIntegrationComponentSurface surface);

        /// <summary>
        /// This will find an instance of a component integration surface within the registry.
        /// </summary>
        /// <typeparam name="TComponentSurface">Type to find</typeparam>
        /// <returns></returns>
        TComponentSurface Find<TComponentSurface>() where TComponentSurface : AbstractIntegrationComponentSurface;
    }
}