using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;

namespace Queue
{
    class Client
    {
        private StopSignal _signal;

        public string SessionId { get; set; }

        public Client(StopSignal signal, string sessionId)
        {
            _signal = signal;
            SessionId = sessionId;
        }

        public void Do()
        {
            Encoding encoding = Encoding.Unicode;

            using (var context = NetMQContext.Create())
            {
                using (var client = context.CreateRequestSocket())
                {
                    client.Connect(Config.ServerFrontendAddr);
                    client.Options.Identity = encoding.GetBytes("t_" + Thread.CurrentThread.ManagedThreadId.ToString());

                    while (!_signal.CanStop())
                    {
                        client.SendMore(encoding.GetBytes(SessionId));
                        client.SendMore(string.Empty);
                        client.Send(encoding.GetBytes("fuck_zeromq_where_is_header_!!!"));
                        

                        var answer = new ZMessage(client);
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ": answer=" + answer.BodyToString());
                        
                        Thread.Sleep(500);
                    }
                }
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ": client stopped!");
        }
    }
}
