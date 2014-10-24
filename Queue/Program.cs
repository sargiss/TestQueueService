using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Communication;
using Queue.Communication.Dto;

namespace Queue
{
    class Program
    {
        static void Main(string[] args)
        {
            var signal = new StopSignal();

            var server = new OperatorSessionServiceBroker(signal);

            Task.Run(new Action(server.Do));
            Task.Run(new Action(new SearchTicketServiceWorker().Do));

            var sm = new OperatorSessionManager();
            var cp = new ConnectionPool();
            var handler = new RequestHandler(sm, new TicketStateManager(), cp);

            for (var i = 0; i < 5; i++)
            {
                Task.Run(new Action(new Worker(handler).Do));
            }

            for (var i = 0; i < 1; i++)
            {
                Task.Run(new Action(() =>
                {
                    var sessionId = sm.Login("u", "p");
                    for (var c = 0; c < 10; c++)
                    {
                        using (var client = cp.GetClient<OperatorSessionServiceClient>())
                        {
                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ChangeState,
                                SessionId = sessionId,
                                OptionalParam = SessionStatus.Free
                            });

                            while (sm.GetSession(sessionId).TicketId == 0) { }

                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ConfirmCall,
                                SessionId = sessionId
                            });

                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ChangeState,
                                SessionId = sessionId,
                                OptionalParam = SessionStatus.Busy
                            });

                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ChangeState,
                                SessionId = sessionId,
                                OptionalParam = SessionStatus.Free
                            });


                        }

                    }
                    using (var client = cp.GetClient<OperatorSessionServiceClient>())
                    {
                        client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                    {
                        Event = SessionEventType.Logout,
                        SessionId = sessionId
                    });
                    }
                }));
            }

            char k = 'a';
            do
            {
                k = (char)Console.Read();
            }
            while (k != 'q');
            signal.Stop();
            Console.ReadKey();
        }
    }
}
