using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;
using Queue.Communication.Dto;

namespace Queue
{
    class Worker
    {
        RequestHandler _handler;

        public Worker(RequestHandler handler)
        {
            _handler = handler;
        }

        public void Do()
        {
            using (var context = NetMQContext.Create())
            {
                using (var worker = context.CreateResponseSocket())
                {
                    worker.Connect(Config.ServerBackendAddr);

                    var poller = new Poller(new[] { worker });

                    worker.ReceiveReady += (s, e) =>
                    {
                        var msg = new ZMessage(e.Socket);
                        
                        var addr = Encoding.Unicode.GetString(msg.Address);
                        Console.WriteLine("Worker: get msg from " + addr);

                        if (msg.BodyToString() == "STOP")
                        {
                            poller.Stop();
                            Console.WriteLine("Worker stopping");
                        }

                        HandleRequest(msg);

                        msg.StringToBody("OK-" + addr);
                        msg.Send(worker);
                    };

                    poller.Start();
                }
            }
            Console.WriteLine("Worker stopped");
        }

        private void HandleRequest(ZMessage msg)
        {
            var body = msg.BodyToString();
            var request = JsonConvert.DeserializeObject<OperatorSessionEventMsg>(body);

            _handler.Process(request);
        }
    }
}
