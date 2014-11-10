using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Queue.Communication;
using Queue.Communication.Dto;
using NetMQ;

namespace Queue
{
    class Program
    {
        static void Main(string[] args)
        {
            f();
        }

        static void f()
        {
            var startEvt = new EventWaitHandle(false, EventResetMode.ManualReset);
            var signal = new StopSignal();

            var sm = new OperatorSessionManager();
            var cp = new ConnectionPool();

            var server = new OperatorSessionServiceBroker(signal);
            Task.Run(new Action(server.Do));
            Task.Run(new Action(new SearchTicketServiceWorker(new SearchTicketManager(0,0,0,0,0), sm, cp).Do));

            System.Threading.Thread.Sleep(3000);

            var handler = new RequestHandler(sm, new TicketStateManager(), cp);

            for (var i = 0; i < 10; i++)
            {
                Task.Run(new Action(new Worker(handler).Do));
            }

            for (var i = 0; i < 40; i++)
            {
                Task.Run(new Action(() =>
                {
                    TimeSpan t1 = TimeSpan.Zero, t2 = TimeSpan.Zero, t3 = TimeSpan.Zero;
                    
                    var r = new Random(DateTime.Today.Millisecond % (Thread.CurrentThread.ManagedThreadId + 1));
                    startEvt.WaitOne();
                    System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                    w.Start();
                    Console.WriteLine(" START");
                    var sessionId = sm.Login("u", "p");
                    var start = DateTime.Now;
                    int count = 800;
                    for (var c = 0; c < count; c++)
                    {
                        using (var client = cp.GetClient<OperatorSessionServiceClient>())
                        {
                            var e = w.Elapsed;
                            //Console.WriteLine(w.Elapsed + " FREE");
                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ChangeState,
                                SessionId = sessionId,
                                OptionalParam = SessionStatus.Free
                            });
                            t1 += w.Elapsed - e;
                            //Console.WriteLine(w.Elapsed + " Wait");
                            //while (sm.GetSession(sessionId).TicketId == 0) { }
                            //Console.WriteLine(w.Elapsed + " Call");
                            e = w.Elapsed;
                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ConfirmCall,
                                SessionId = sessionId
                            });
                            t2 += w.Elapsed - e;
                            Thread.Sleep(r.Next(0, 90));

                            e = w.Elapsed;
                            //Console.WriteLine(w.Elapsed + " Busy");
                            client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                            {
                                Event = SessionEventType.ChangeState,
                                SessionId = sessionId,
                                OptionalParam = SessionStatus.Busy
                            });
                            t3 += w.Elapsed - e;
                        }

                    }
                    //Console.WriteLine(w.Elapsed + " Logout");
                    using (var client = cp.GetClient<OperatorSessionServiceClient>())
                    {
                        client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                        {
                            Event = SessionEventType.Logout,
                            SessionId = sessionId
                        });
                    }
                    var end = DateTime.Now;
                    w.Stop();

                    var avr = w.Elapsed.Ticks / count;
                    t1 = new TimeSpan(t1.Ticks / count);
                    t2 = new TimeSpan(t2.Ticks / count);
                    t3 = new TimeSpan(t3.Ticks / count);
                    //Console.WriteLine(string.Format("{0} END: t1={1}, t2={2}, t3={3}", 
                      //  new TimeSpan(avr), t1, t2, t3));
                    Console.WriteLine("{0} - {1}", start.ToString("hh:mm:ss.fff tt"),
                        end.ToString("hh:mm:ss.fff tt"));
                }));
            }

            startEvt.Set();

            char k = 'a';
            do
            {
                k = (char)Console.Read();
            }
            while (k != 'q');
            signal.Stop();
            
        }

        static void f2()
        {
            var a = new A();

            new Task(a.P).Start();
            Thread.Sleep(2000);
            new Task(a.S).Start();

            Console.ReadKey();
        }
    }
}


class A
{
    string adr = "tcp://localhost:7979";

    public void S()
    {
        using (var c = NetMQContext.Create())
        {
            using (var s = c.CreateSubscriberSocket())
            {
                s.Connect(adr);
                s.Subscribe("");

                Console.WriteLine("S: connected");

                while (true)
                {
                    Console.WriteLine("S: read...");
                    var m = s.ReceiveString();
                    Console.WriteLine(m);
                }
            }
        }
    }

    public void P()
    {
        using (var c = NetMQContext.Create())
        {
            using (var p = c.CreatePublisherSocket())
            {
                p.Bind(adr);
                Console.WriteLine("P: bound");
                Thread.Sleep(4000);

                var i=0;
                while (i<5)
                {
                    Console.WriteLine("P: write " + i);
                    p.Send("fuck" + i);
                    i++;
                }
            }
        }
    }
}