using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Carbon.Core.Internals.MessageResolution
{
    /// <summary>
    /// Maps a message to a method on a component for invocation.
    /// </summary>
    public class MapMessageToMethod : IMapMessageToMethod
    {
        private readonly string m_method_name;

        public MapMessageToMethod()
            : this(string.Empty)
        {

        }

        public MapMessageToMethod(string methodName)
        {
            m_method_name = methodName;
        }

        public MethodInfo Map(object customComponent, IEnvelope message)
        {
            // default signature of Process(IMessage message), so look at the payload:
            return Map(customComponent, message as object);
        }

        public MethodInfo Map(object customComponent, object message)
        {
            var methods = new List<MethodInfo>();

            if (!string.IsNullOrEmpty(m_method_name))
            {
                try
                {
                    return customComponent.GetType().GetMethod(m_method_name.Trim());
                }
                catch (AmbiguousMatchException ambiguousMatchException)
                {
                    // one or more methods on the component share the same name
                    // must look through all of the methods to match the message
                    // (defer to the code below):
                }
            }
           
            object messageToInspect = null;

            if (typeof(IEnvelope).IsAssignableFrom(message.GetType()))
            {
                messageToInspect = ((IEnvelope)message).Body.Payload;
            }
            else
            {
                messageToInspect = message;
            }

            foreach (var method in customComponent.GetType().GetMethods())
            {
                // ignore properties translated to method implementations and native System.Object methods:
                if(method.Name.StartsWith("set_")) continue;
                if(method.Name.StartsWith("ToString")) continue;
                if (method.Name.StartsWith("Equals")) continue;

                foreach (var parameter in method.GetParameters())
                {
                    // check for generics (IList<T>, IRepository<T>, etc...)
                    if (parameter.ParameterType.IsGenericType && messageToInspect.GetType().IsGenericType)
                    {
                        var parameterGenericInstance = parameter.ParameterType.GetGenericArguments()[0];
                        var messageToMatchGenericInstance =
                            messageToInspect.GetType().GetGenericArguments()[0];

                        if (parameterGenericInstance == messageToMatchGenericInstance)
                            methods.Add(method);
                    }

                    // check interfaces and base classes:
                    else if (parameter.ParameterType.IsInterface & !parameter.ParameterType.IsGenericType)
                    {
                        var parameterType = parameter.ParameterType;
                        if (parameterType.IsAssignableFrom(messageToInspect.GetType()))
                            methods.Add(method);
                    }

                    // native object passed:
                    else
                    {
                        if (parameter.ParameterType == messageToInspect.GetType())
                            methods.Add(method);
                    }
                   
                }

            }

            // check the mapped results:
            if(methods.Count > 1)
                throw new AmbiguousMatchException(string.Format("The message type '{0}' could not be matched to a single method for invocation on component '{1}'. Try specifying the exact method name that should be invoked to aid in message to method resolution.", 
                    messageToInspect.GetType().FullName, customComponent.GetType().FullName));

            if (methods.Count == 0)
                throw new ArgumentException(string.Format("The message type '{0}' could not be matched to any method for invocation on component '{1}'.",
                    messageToInspect.GetType().FullName, customComponent.GetType().FullName));

            return methods[0];
        }

    }
}