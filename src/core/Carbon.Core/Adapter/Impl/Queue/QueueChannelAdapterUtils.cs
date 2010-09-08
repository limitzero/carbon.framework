using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core.Adapter.Impl.Queue
{
    public class QueueChannelAdapterUtils
    {
        static QueueChannelAdapterUtils()
        {
        }

        /// <summary>
        /// This will extract the channel name from the uri location.
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="location">Uri directory definition for queue transport</param>
        /// <returns></returns>
        public static string RetreiveLocationFromProtocolUri(string scheme, string location)
        {
            var result = location;
            var adjustedScheme = string.Concat(scheme, "://");

            if (location.StartsWith(adjustedScheme))
            {
                result = result.Replace(adjustedScheme, string.Empty);
            }

            return result;

        }
    }
}
