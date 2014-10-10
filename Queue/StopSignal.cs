using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Queue
{
    class StopSignal
    {
        EventWaitHandle _stop = new EventWaitHandle(false, EventResetMode.ManualReset);

        public void Stop()
        {
            _stop.Set();
        }

        public void WaitForStop()
        {
            _stop.WaitOne();
        }

        public bool CanStop()
        {
            return _stop.WaitOne(100);
        }
    }
}
