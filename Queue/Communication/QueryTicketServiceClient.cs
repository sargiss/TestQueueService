using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Queue.Dto;

namespace Queue
{
    class QueryTicketServiceClient: IDisposable
    {
        private static Lazy<NetMQContext> _context = new Lazy<NetMQContext>(() =>
        {
            return NetMQContext.Create();
        });

        private static NetMQContext Context { get { return _context.Value; } }

        public void QueryNextTicket()
        {
            using (var client = Context.CreatePublisherSocket())
            {
                client.Connect(Config.QueryServerAddr);

                var query = new QueryTicketMsg();
                var msg = new ZMessage(query);
                msg.Send(client);
            }
        }

        public void Dispose()
        {
            if (_context.IsValueCreated)
                _context.Value.Dispose();
        }
    }
}
