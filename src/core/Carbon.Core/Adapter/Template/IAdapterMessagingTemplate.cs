using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Templates.Messaging;

namespace Carbon.Core.Adapter.Template
{
    /// <summary>
    /// Contract that all adapters will use for sending and receiving messages
    /// </summary>
    public interface IAdapterMessagingTemplate : IMessagingTemplate<Uri>
    {
    }
}