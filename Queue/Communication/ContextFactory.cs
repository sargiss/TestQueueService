using System;
using System.Collections.Generic;
using System.Linq;
using NetMQ;

namespace Queue.Communication
{
    class ContextFactory
    {
        Lazy<NetMQContext> _context = new Lazy<NetMQContext>(NetMQContext.Create);

        public NetMQContext GetContext()
        {
            return _context.Value;
        }
    }
}
