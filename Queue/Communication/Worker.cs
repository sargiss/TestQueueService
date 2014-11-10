using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;
using Queue.Communication.Dto;

namespace Queue.Communication
{
    class Worker
    {
        RequestHandler _handler;
        NetMQSocket _worker;

        public Worker(RequestHandler handler)
        {
            _handler = handler;
        }

        public void Do()
        {
            using (var context = NetMQContext.Create())
            {
                using (ReconnectToBroker(context))
                {
                    _worker.Connect(Config.ServerBackendAddr);

                    var poller = new Poller(new[] { _worker });

                    _worker.ReceiveReady += (s, e) =>
                    {
                        var zmsg = new ZMessage(e.Socket);
                        HandleRequest(zmsg);
                    };

                    poller.Start();
                }
            }
            Console.WriteLine("Worker stopped");
        }

        NetMQSocket ReconnectToBroker(NetMQContext ctx)
        {
            if (_worker != null)
            {
                _worker.Dispose();
            }
            _worker = ctx.CreateDealerSocket();
            _worker.Connect(Config.BrokerAddr);

            SendToBroker(CommunicationPrimitives.Commands.READY, null);

            return _worker;
        }

        private void SendToBroker(string cmd, ZMessage zmsg)
        {

        }

        private void HandleRequest(ZMessage msg)
        {
            var body = msg.BodyToString();
            var request = JsonConvert.DeserializeObject<OperatorSessionEventMsg>(body);

            _handler.Process(request);
        }
    }
}
