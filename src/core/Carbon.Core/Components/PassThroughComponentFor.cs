using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core.Components
{
    /// <summary>
    /// Pass-through component used to accept the message from the source and pass it out to the target unaltered.
    /// </summary>
    public class PassThroughComponentFor<T>
    {
        public virtual T PassThrough(T message)
        {
            return message;
        }
    }
}
