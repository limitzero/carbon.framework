using System.ComponentModel;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Registry
{
    /// <summary>
    /// Contract representing the registration strategy for the adapter.
    /// </summary>
    public interface IAdapterRegistrationStrategy
    {
        /// <summary>
        /// (Read-Write). This will set the container instance that can be used to resolve or add resources needed for the adapter.
        /// </summary>
        IObjectBuilder ObjectBuilder { get; set; }

        /// <summary>
        /// This will configure the adapter and set
        /// </summary>
        IAdapterConfiguration Configure();
    }
}