using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace Queue.Communication
{
    public class TaskManagerServiceWorker
    {
        NetMQContext _context;
        IRequestHandler _handler;

        public TaskManagerServiceWorker(ContextFactory factory, IRequestHandler handler)
        {
            _context = factory.GetContext();
            _handler = handler;
        }

        public void Do()
        {
            using(var worker = _context.CreateResponseSocket())
            {
                worker.Bind(Config.TaskManagerAddr);
                while(true)
                {
                    var zmsg = new ZMessage(worker);
                    var reply =_handler.Process(zmsg);
                    reply.Send(worker);
                }
            }
        }
    }
}
