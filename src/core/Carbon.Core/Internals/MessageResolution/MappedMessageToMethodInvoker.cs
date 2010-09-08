using System;
using System.Reflection;

namespace Carbon.Core.Internals.MessageResolution
{
    /// <summary>
    /// This will invoke a method that is mapped to a particular message
    /// on a component instance.
    /// </summary>
    public class MappedMessageToMethodInvoker : IMappedMessageToMethodInvoker
    {
        private readonly object m_instance;
        private readonly MethodInfo m_method;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        public MappedMessageToMethodInvoker(object instance, MethodInfo method)
        {
            m_instance = instance;
            m_method = method;
        }

        public IEnvelope Invoke()
        {
            IEnvelope exchange = null;

            try
            {
                object result = null;

                exchange = new Envelope();

                result = m_method.Invoke(m_instance, null);
                exchange = CreateEnvelopeFromResult(exchange, result);

            }
            catch 
            {
                throw;
            }

            return exchange;
        }


        public IEnvelope Invoke(object message)
        {
            IEnvelope retval = null;

            try
            {
                object result = null;

                if (m_method.GetParameters().Length > 1)
                {
                    string msg =
                        string.Format(
                            "An error has ocurred while attempting to invoke the method '{0}' on the component '{1}'. The components that participate in the messaging infrastructure can only have zero or one input parameters.",
                            m_method.Name, this.m_instance.GetType().FullName);
                    throw new Exception(msg);
                }

                // pass the envelope:
                if (m_method.GetParameters()[0].ParameterType == typeof(IEnvelope)
                    && typeof(IEnvelope).IsAssignableFrom(message.GetType()))
                {
                    retval = message as IEnvelope;
                    result = m_method.Invoke(m_instance, new object[] { message });
                }

                // pass as-is:
                if (m_method.GetParameters()[0].ParameterType != typeof(IEnvelope) &&
                    !typeof(IEnvelope).IsAssignableFrom(message.GetType()))
                {
                    retval = new Envelope();
                    result = m_method.Invoke(m_instance, new object[] { message });
                }

                // pass the contents of the envelope:
                if (m_method.GetParameters()[0].ParameterType != typeof(IEnvelope) &&
                    typeof(IEnvelope).IsAssignableFrom(message.GetType()))
                {
                    retval = new Envelope();
                    var payload = ((IEnvelope)message).Body.GetPayload<object>();

                    if (m_method.GetParameters()[0].ParameterType == payload.GetType())
                        result = m_method.Invoke(m_instance, new object[] { payload });

                    // interface based-messages:
                    if (m_method.GetParameters()[0].ParameterType.IsInterface &&
                          m_method.GetParameters()[0].ParameterType.IsAssignableFrom(payload.GetType()))
                        result = m_method.Invoke(m_instance, new object[] { payload });
                }

                retval = CreateEnvelopeFromResult(retval, result);
            }
            catch 
            {
                throw;
            }

            return retval;
        }

        private IEnvelope CreateEnvelopeFromResult(IEnvelope envelope, object result)
        {
            if (result != null)
            {
                if (!typeof(IEnvelope).IsAssignableFrom(result.GetType()))
                {
                    envelope.Body.SetPayload(result);
                }
                else
                {
                    envelope = result as IEnvelope;
                }
            }
            else
            {
                envelope = new NullEnvelope();
                //envelope.Body.SetPayload(result);
            }

            return envelope;
        }
    }
}