using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Carbon.ESB.Internals
{
    /// <summary>
    /// General contract for all on-demand scanning of components
    /// </summary>
    public interface IComponentScanner
    {
        ReadOnlyCollection<Type> Components { get; }
        void ScanLocation(string directory);
        void Scan(params string[] assemblies);
        void Scan(params Assembly[] assemblies);
    }
}