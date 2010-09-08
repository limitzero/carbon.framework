using Carbon.Core.RuntimeServices;

namespace Carbon.ESB.Services
{
    /// <summary>
    /// Base class for all services that will be attached to the context and run in the background. 
    /// </summary>
    public abstract class ContextBackgroundService : 
        AbstractBackgroundService, IContextBackgroundService
    {
        /// <summary>
        /// (Read-Only). The current context for accessing system resources.
        /// </summary>
        public IMessageBus Bus { get;  set; }
    }
}