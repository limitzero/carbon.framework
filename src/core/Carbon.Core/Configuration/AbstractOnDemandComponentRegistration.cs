using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Builder;

namespace Carbon.Core.Configuration
{
    public abstract class AbstractOnDemandComponentRegistration
    {
        public IObjectBuilder Builder { get; set; }
        public abstract void Register();
    }
}
