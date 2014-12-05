using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Communication;

namespace Queue
{
    class WorkerMessageHandler: IRequestHandler
    {
        public ZMessage Process(ZMessage zmsg)
        {
            var reply = new ZMessage();
            reply.Push(ReplyStatus.OK);
            return reply;
        }
    }
}
