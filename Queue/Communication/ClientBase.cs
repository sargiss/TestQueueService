using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMQ;

namespace Queue.Communication
{
    class ClientBase
    {
        protected Encoding _encoding = Encoding.Unicode;
        protected NetMQSocket _client;
        protected IRequestHandler _handler;
        protected NetMQContext _context;
    }
}
