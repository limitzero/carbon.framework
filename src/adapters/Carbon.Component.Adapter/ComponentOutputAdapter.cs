using System;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;
using Carbon.Core.Internals.MessageResolution;

namespace Carbon.Component.Adapter
{
    /// <summary>
    /// Adapter for taking messages from a channel and calling the component method with the message.
    /// Addressing Scheme: direct://{component identifier}/?method={component method name}&channel={name of the c}
    /// </summary>
    public class ComponentOutputAdapter : AbstractOutputChannelAdapter
    {
        private object _component = null;
        private string _method_name = string.Empty;
        private string _output_channel = string.Empty;

        public ComponentOutputAdapter(IObjectBuilder builder) : base(builder)
        {
        }

        public override void DoStartActivities()
        {
            var pairs = Utils.CreateNameValuePairsFromUri(this.Uri);
            var component_id = new Uri(this.Uri).Host;

            _method_name = pairs["method"];
            _output_channel = pairs["channel"];

            if (string.IsNullOrEmpty(component_id))
                throw new Exception("For the component output adapter, the component identifier was not specified as the host in the uri addressing scheme. " +
                    "Scheme: direct://{component id}/?method={method on component}&channel={name of channel to place message after invocation}");

            if (string.IsNullOrEmpty(_method_name))
                throw new Exception("For the component output adapter, the method name  was not specified a querystring value in the uri addressing scheme. " +
                    "Scheme: direct://{component id}/?method={method on component}&channel={name of channel to place message after invocation}");

            if (string.IsNullOrEmpty(_output_channel))
                throw new Exception("For the component output adapter, the channel name  was not specified a querystring value in the uri addressing scheme. " +
                    "Scheme: direct://{component id}/?method={method on component}&channel={name of channel to place message after invocation}");

            _component = base.ObjectBuilder.Resolve(component_id);

            if (_component == null)
                throw new Exception("For the component output adapter, the component specified with the identifier '" +
                                    component_id + "' in the container could not be resolved.");

            // can only treat classes that act as message consumers when the method has one and only one input parameters:
            if (!(_component.GetType().GetMethod(_method_name).GetParameters().Length == 1))
                throw new Exception(
                 string.Format(
                     "The following method '{0}' on component '{1}' does not have an implementation with one input " +
                     "parameter to accept the message from the component output adapter and has been stopped. " +
                     "Please change the signature of the component to 'public void {2}([your message type] message)'.",
                     _method_name,
                     _component.GetType().FullName,
                     _method_name));

            SetChannel(_output_channel);
        }

        public override void DoSend(IEnvelope envelope)
        {
            SendMessage(envelope);
        }

        private void SendMessage(IEnvelope message)
        {
            try
            {
                SubmitMessage(this.Uri, message);
            }
            catch 
            {
                Utils.Retry(base.ObjectBuilder, (arg1, arg2) => this.SubmitMessage(this.Uri, message),
                            this.Uri, message, base.RetryStrategy);
            }
        }

        private void SubmitMessage(string destination, IEnvelope message)
        {

            if (_component == null || string.IsNullOrEmpty(_method_name))
                return;

            try
            {
                var mapper = new MapMessageToMethod();
                var method = mapper.Map(_component, message);

                var invoker = new MappedMessageToMethodInvoker(_component, method);
                invoker.Invoke(method); // no response here to be forwared!!!
            }
            catch (Exception exception)
            {
                throw;
            }
        }

    }
}