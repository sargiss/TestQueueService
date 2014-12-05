using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    class WorkerClient: ClientBase, IServiceClient
    {
        public void Connect()
        {
            _client = _context.CreateRequestSocket();
            _client.Connect(Config.TaskManagerAddr);
        }

        public void Close()
        {
            _client.Dispose();
        }

        public void Send(ZMessage zmsg)
        {
            zmsg.Send(_client);
        }

        public ZMessage Recieve()
        {
            var zmsg = new ZMessage(_client);
            return zmsg;
        }
    }
}
