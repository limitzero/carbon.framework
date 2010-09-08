using System;
using System.Reflection;
using LinFu.Reflection;

namespace Kharbon.Core.Internals.Reflection
{
    // LinFu for Dynamic Proxy Generation: http://www.codeproject.com/KB/cs/LinFuPart1.aspx

    /// <summary>
    /// Proxy builder for messages that are constructed solely from interfaces for multi-inheritance.
    /// </summary>
    public class InterfaceProxyBuilder
    {
        public TContract BuildProxy<TContract>()
        {
            return (TContract) this.BuildProxy(typeof (TContract));
        }

        public object BuildProxy(Type component)
        {
            var dynamicObject = new DynamicObject(new object());

            var proxy = dynamicObject.CreateDuck(component);

            return proxy;
        }


    }
}
