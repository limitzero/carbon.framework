using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core
{
    public class Constants
    {
        static Constants()
        {
        }

        public const string INPUT_ADAPTER_SUFFIX = "-in";
        public const string OUTPUT_ADAPTER_SUFFIX = "-out";

        /// <summary>
        /// Collection of standard system uri's.
        /// </summary>
        public class SystemUris
        {
            static SystemUris()
            {
            }

            /// <summary>
            /// Uri for sending messages to the console (i.e. standard I/O).
            /// </summary>
            public const string STDIO_URI = "stdio://local";
        }


        /// <summary>
        /// Collection of uri's for logging messages.
        /// </summary>
        public class LogUris
        {
            static LogUris()
            {
            }
            public const string DEBUG_LOG_URI = "log://carbon/?level=debug";
            public const string INFO_LOG_URI = "log://carbon/?level=info";
            public const string WARN_LOG_URI = "log://carbon/?level=warn";
            public const string ERROR_LOG_URI = "log://carbon/?level=error";
            public const string FATAL_LOG_URI = "log://carbon/?level=fatal";

        }

    }
}