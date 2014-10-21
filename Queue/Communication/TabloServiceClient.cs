using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Queue.Dto;

namespace Queue
{
    public class TabloServiceClient
    {
        private static Lazy<NetMQContext> _context = new Lazy<NetMQContext>(() =>
        {
            return NetMQContext.Create();
        });

        private static NetMQContext Context { get { return _context.Value; } }

        public void Dispose()
        {
            if (_context.IsValueCreated)
                _context.Value.Dispose();
        }

        public void UpdateTablo(OperatorSession session)
        {
            using (var client = Context.CreatePublisherSocket())
            {
                client.Connect(Config.TabloServerAddr);

                var msg = new ZMessage(new TabloMsg());
                msg.Send(client);
            }
        }
    }
}
