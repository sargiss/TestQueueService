using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;
using Queue.Dto;

namespace Queue
{
    class Worker
    {
        OperatorSessionManager _sessionManager= new OperatorSessionManager();

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
                        //Console.WriteLine("Worker: body=" + msg.BodyToString());

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
            var request = JsonConvert.DeserializeObject<OperatorSessionEventMsg>(msg.BodyToString());

            var session = OperatorSessionManager.Instance.GetSession(request.SessionId);

            if (session == null)
                return;

            new RequestHandler().Process(session, request);
        }
    }
}
