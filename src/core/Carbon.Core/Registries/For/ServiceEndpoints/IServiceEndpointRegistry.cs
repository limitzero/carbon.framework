using System;
using Carbon.Core.Stereotypes.For.Components.Service.Impl;

namespace Carbon.Core.Registries.For.ServiceEndpoints
{
    /// <summary>
    /// Contract for registering all service endpoints.
    /// </summary>
    public interface IServiceEndpointRegistry : IRegistry<IServiceActivator, Guid>
    {
 
    }
}