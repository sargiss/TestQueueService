using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMQ;

namespace Queue.Communication
{
    class ZMessageExtended: ZMessage
    {
        public ZMessageExtended(NetMQSocket socket)
            : base(socket)
        { }

        
    }
}
