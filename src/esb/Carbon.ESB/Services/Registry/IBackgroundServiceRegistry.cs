using Carbon.Core.Registries;
using Carbon.Core.RuntimeServices;
using Kharbon.Core;

namespace Carbon.ESB.Services.Registry
{
    /// <summary>
    /// Contract for registry to holding all of the background services that the infrastructure will use.
    /// </summary>
    public interface IBackgroundServiceRegistry : IStartable, IRegistry<ContextBackgroundService, string>
    {
        void SetMessageBus(IMessageBus bus);
    }
}