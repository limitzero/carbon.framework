using System;
using Carbon.Core;

namespace Carbon.Component.Adapter
{
    public class ComponentAdapterUtils
    {
        static ComponentAdapterUtils()
        {
        }

        /// <summary>
        /// This will extract the directory location from the uri definition
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="location">Uri directory definition for file transport</param>
        /// <returns></returns>
        public static string RetreiveLocationFromProtocolUri(string scheme, string location)
        {
            var result = location;
     
            // clean up the location uri:
            var uriParts = location.Split(new char[] {'?'});
            var uri = uriParts[0];

            // remove the trailing "/" if provided:
            if (uri.EndsWith("/"))
                uri = uri.TrimEnd(new char[]{'/'});

            // replace the channel specific scheme:
            if (uri.StartsWith(scheme))
                uri = uri.Replace(string.Concat(scheme,"://"), string.Empty);

            result = uri.Trim();

            return result;

        }

        public static string CreateDefaultFileName(string messageid, string correlationid)
        {
            var format = string.Format("FILE-MSG(MSG-ID[{0}]CORR-ID[{1}]).txt", messageid, correlationid);
            return format;
        }

        /// <summary>
        /// This will return the message identifier and the correlation identifier for 
        /// a file message (if defined);
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Tuple<string, string> GetDefaultFileIdentifiers(string fileName)
        {
          
            var message_id = string.Empty;
            var correlation_id = string.Empty;
            var startPos = 0;
            var endPos = 0;

            try
            {
                // get message id:
                startPos = fileName.IndexOf("[", StringComparison.InvariantCulture);
                endPos = fileName.IndexOf("]", startPos, StringComparison.InvariantCulture);
                message_id = fileName.Substring(startPos + 1, endPos - (startPos + 1));
            }
            catch
            {
            }

            try
            {
                // get correlation id:
                startPos = fileName.IndexOf("[", endPos, StringComparison.InvariantCulture);
                endPos = fileName.IndexOf("]", startPos, StringComparison.InvariantCulture);
                correlation_id = fileName.Substring(startPos + 1, endPos - (startPos + 1));
            }
            catch
            {
            }

            return new Tuple<string, string>(message_id, correlation_id);
        }
    }
}