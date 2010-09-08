using System;
using Carbon.Core.Builder;

namespace Carbon.ESB.Configuration
{
    /// <summary>
    /// Bootstrapper for initializing internal components for custom configuration by the framework.
    /// </summary>
    public abstract class AbstractBootStrapper
    {
        /// <summary>
        /// (Read-Write). Reference to the object builder for storing/retrieving any resources.
        /// </summary>
        public IObjectBuilder Builder { get; set; }

        /// <summary>
        /// This will examine the bootstrapper to see if the component 
        /// being passed is a match for the component to be custom
        /// configured.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public  abstract bool IsMatchFor(Type component);

        /// <summary>
        /// This will configure the components based on conventions.
        /// </summary>
        public abstract void Configure();
    }
}
