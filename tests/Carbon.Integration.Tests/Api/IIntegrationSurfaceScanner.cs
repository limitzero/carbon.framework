using System;
using System.Collections.ObjectModel;

namespace Carbon.Integration.Tests.Api
{
    public interface IIntegrationSurfaceScanner
    {
        void Scan();
        ReadOnlyCollection<Type> Surfaces { get; }
    }
}