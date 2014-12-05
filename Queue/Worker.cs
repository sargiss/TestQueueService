using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Communication;

namespace Queue
{
    class Worker
    {
        ContextFactory _factory;
        IRequestHandler _handler;
        IServiceClient _taskManagerClient;

        public Worker (ContextFactory factory, IRequestHandler handler)
        {
            _factory = factory;
            _handler = handler;
        }

        public void Do()
        {
            var api = new WorkerApi(_factory);

            api.Connect();

            while (true)
            {
                var msg = api.Recieve();
                var reply = _handler.Process(msg);
                var status = reply.PopStr();
                api.Send(ReplyStatus.OK, reply);
            }
        }
    }
}
