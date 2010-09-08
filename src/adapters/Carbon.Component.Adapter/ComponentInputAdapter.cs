using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;
using Carbon.Core.Internals;
using Carbon.Core.Internals.MessageResolution;

namespace Carbon.Component.Adapter
{
    /// <summary>
    /// Adapter for taking messages from a component method and loading them onto a channel.
    /// Addressing Scheme: direct://{component identifier}/?method={component method name}&channel={name of the c}
    /// </summary>
    public class ComponentInputAdapter : AbstractInputChannelAdapter
    {
        private object _component = null;
        private string _method_name = string.Empty;
        private string _input_channel = string.Empty;

        public ComponentInputAdapter(IObjectBuilder builder)
            : base(builder)
        {

        }

        public override void DoStartActivities()
        {
            var pairs = Utils.CreateNameValuePairsFromUri(this.Uri);
            var component_id = new Uri(this.Uri).Host;

            _method_name = pairs["method"];
            _input_channel = pairs["channel"];

            if (string.IsNullOrEmpty(component_id))
                throw new Exception("For the component input adapter, the component identifier was not specified as the host in the uri addressing scheme. " +
                    "Scheme: direct://{component id}/?method={method on component}&channel={name of channel to place message after invocation}");

            if (string.IsNullOrEmpty(_method_name))
                throw new Exception("For the component input adapter, the method name  was not specified a querystring value in the uri addressing scheme. " +
                    "Scheme: direct://{component id}/?method={method on component}&channel={name of channel to place message after invocation}");

            if (string.IsNullOrEmpty(_input_channel))
                throw new Exception("For the component input adapter, the channel name  was not specified a querystring value in the uri addressing scheme. " +
                    "Scheme: direct://{component id}/?method={method on component}&channel={name of channel to place message after invocation}");

            _component = base.ObjectBuilder.Resolve(component_id);

            if (_component == null)
                throw new Exception("For the component input adapter, the component specified with the identifier '" +
                                    component_id + "' in the container could not be resolved.");

            // can only treat classes that act as message producers when the method has no input parameters:
            if (_component.GetType().GetMethod(_method_name).GetParameters().Length > 0)
                throw new Exception(
                    string.Format(
                        "The following method '{0}' on component '{1}' does not have an implementation with one input " +
                        "parameter to accept the message from the component output adapter and has been stopped. " +
                        "Please change the signature of the component to 'public void {2}((your message type) message)'.",
                        _method_name,
                        _component.GetType().FullName,
                        _method_name));

            SetChannel(_input_channel);

        }

        public override void DoStopActivities()
        {
            _component = null;
        }

        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            var contents = this.ExtractMessageContents();
            var header = this.CreateMessageHeader();
            var tuple = new Tuple<IEnvelopeHeader, byte[]>(header, contents);

            IEnvelope message = new NullEnvelope();

            if (_component == null || string.IsNullOrEmpty(_method_name))
            {
                SetMessageForReceive(message);
                return tuple;
            }

            try
            {
                var invoker = new MappedMessageToMethodInvoker(_component, _component.GetType().GetMethod(_method_name));
                message = invoker.Invoke();

                if (!(message is NullEnvelope))
                    base.SetMessageForReceive(message);
            }
            catch (Exception exception)
            {
                throw;
            }

            return tuple;
        }

        public override byte[] ExtractMessageContents()
        {
            return new byte[] { };
        }

        public override IEnvelopeHeader CreateMessageHeader()
        {
            return new EnvelopeHeader();
        }

    }
}