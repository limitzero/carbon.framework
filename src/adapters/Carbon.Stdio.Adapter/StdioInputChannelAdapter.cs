using System;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;

namespace Carbon.Stdio.Adapter
{
    /// <summary>
    ///  Addressing scheme: stdio://local/?prompt={any message}
    /// </summary>
    public class StdioInputChannelAdapter : AbstractInputChannelAdapter
    {
        public string Prompt { get; set; }

        public StdioInputChannelAdapter(IObjectBuilder builder, string prompt) : base(builder)
        {
            Prompt = prompt;
        }

        public override void DoStartActivities()
        {
            var nameValues = Utils.CreateNameValuePairsFromUri(this.Uri);
            Prompt = string.IsNullOrEmpty(nameValues["prompt"]) == true ? Prompt : nameValues["prompt"];
 
            if (!string.IsNullOrEmpty(this.Prompt))
                Console.WriteLine(Prompt);

        }

        public override byte[] ExtractMessageContents()
        {
            // get the line of text from the stdio console:
            var text = string.Empty;
            text = Console.ReadLine();
            return UTF8Encoding.ASCII.GetBytes(text);
        }

        public override IEnvelopeHeader CreateMessageHeader()
        {
            return new EnvelopeHeader();
        }

        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            var content = this.ExtractMessageContents();
            var header = this.CreateMessageHeader();
            return new Tuple<IEnvelopeHeader, byte[]>(header, content);
        }
    }
}