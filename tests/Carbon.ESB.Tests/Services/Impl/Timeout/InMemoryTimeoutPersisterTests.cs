using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.ESB.Messages;
using Carbon.ESB.Services.Impl.Timeout.Persister;
using Xunit;

namespace Carbon.ESB.Tests.Services.Impl.Timeout
{
    public class InMemoryTimeoutPersisterTests
    {
        private ManualResetEvent _wait = null;
        private ITimeoutsPersister _persister = null;

        public InMemoryTimeoutPersisterTests()
        {
            _persister = new InMemoryTimeoutsPersister();    
        }

        [Fact]
        public void can_register_a_message_for_delayed_delivery_and_select_it_for_delivery_after_expiration()
        {
            _wait = new ManualResetEvent(false);

            var tm = new TimeoutMessage(TimeSpan.FromSeconds(1), new object());
            _persister.Save(tm);

            _wait.WaitOne(TimeSpan.FromSeconds(5));
            _wait.Set();

            Assert.Equal(1, _persister.FindAllExpiredTimeouts().Length);
        }

        [Fact]
        public void can_register_a_timeout_for_a_message_and_cancel_it_for_delivery()
        {
            var timeout = 5;

            _wait = new ManualResetEvent(false);
            var messageToDeliver = new object();

            // register the timeout for the message:
            var tm = new TimeoutMessage(TimeSpan.FromSeconds(timeout), messageToDeliver);
            _persister.Save(tm);

            // need long enough window to find the expired messages:
            _wait.WaitOne(TimeSpan.FromSeconds(timeout *2));
            _wait.Set();

            // cancel the timeout for the message:
            _persister.AbortTimeout(new CancelTimeoutMessage(messageToDeliver));

            Assert.Equal(0, _persister.FindAllExpiredTimeouts().Length);
        }
    }
}
