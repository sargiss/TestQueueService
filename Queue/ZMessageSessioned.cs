using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMQ;

namespace Queue
{
    class ZMessageSessioned:ZMessage
    {
        public ZMessageSessioned(NetMQSocket socket)
            : base(socket)
        { }

        public string SessionId
        {
            get { return encoding.GetString(frames[2]); }
        }
    }
}
