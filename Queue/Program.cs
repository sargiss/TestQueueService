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
            //System.Threading.Thread.Sleep(1000);
            Task.Run(new Action(new SearchTicketServiceWorker(new SearchTicketManager(0,0,0,0,0), sm, cp).Do));

            //System.Threading.Thread.Sleep(2000);

            var handler = new DefaultMessageHandler();

            for (var i = 0; i < 2; i++)
            {
                Task.Run(new Action(new Worker().Do));
            }

            int threads = 17;
            int count = 2;
            int readyToFinish = 0;
            int countOfStarted = 0;

            List<Task> tasks = new List<Task>(threads);
            
            new Thread(() =>
            {
                while (countOfStarted < threads) { }
                Console.WriteLine("LAUNCH!!!!!");
                var w = new System.Diagnostics.Stopwatch();
                w.Start();
                startEvt.Set();
                Task.WaitAll(tasks.ToArray());
                w.Stop();
                Console.WriteLine("TOTAL: " + w.Elapsed);
            }).Start();

            for (var i = 0; i < threads; i++)
            {
                var t = Task.Run(new Action(() =>
                {
                    TimeSpan t1 = TimeSpan.Zero, t2 = TimeSpan.Zero, t3 = TimeSpan.Zero;

                    var r = new Random(DateTime.Today.Millisecond % (Thread.CurrentThread.ManagedThreadId + 1));
                    BrokerClient client = new BrokerClient(handler, CommunicationPrimitives.ClientTypeId);
                    client.count = i;

                    Interlocked.Increment(ref countOfStarted);

                    startEvt.WaitOne();

                    System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                    w.Start();
                    Console.WriteLine(" START");
                    var sessionId = sm.Login("u", "p");
                    var start = DateTime.Now;

                    int c = 0;
                    for (; ; c++ )
                    {

                        var e = w.Elapsed;
                        //Console.WriteLine(w.Elapsed + " FREE");

                        client.SendOperatorCommand(new OperatorSessionEventMsg()
                        {
                            Event = SessionEventType.ChangeState,
                            SessionId = sessionId,
                            OptionalParam = SessionStatus.Free
                        });
                        t1 += w.Elapsed - e;

                        e = w.Elapsed;
                        client.SendOperatorCommand(new OperatorSessionEventMsg()
                        {
                            Event = SessionEventType.ConfirmCall,
                            SessionId = sessionId
                        });
                        t2 += w.Elapsed - e;
                        Thread.Sleep(r.Next(0, 120));

                        e = w.Elapsed;
                        //Console.WriteLine(w.Elapsed + " Busy");
                        client.SendOperatorCommand(new OperatorSessionEventMsg()
                        {
                            Event = SessionEventType.ChangeState,
                            SessionId = sessionId,
                            OptionalParam = SessionStatus.Busy
                        });
                        t3 += w.Elapsed - e;

                        if (c >= count)
                        {
                            if (c == count)
                                Interlocked.Increment(ref readyToFinish);
                            if (readyToFinish == threads)
                                break;
                        }
                        else
                        {
                            Console.WriteLine("ID=" + Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                    client.Close();
                    //Console.WriteLine(w.Elapsed + " Logout");
                    var client1 = new BrokerClient(handler, CommunicationPrimitives.ClientTypeId);

                    client1.SendOperatorCommand(new OperatorSessionEventMsg()
                    {
                        Event = SessionEventType.Logout,
                        SessionId = sessionId
                    });
                    client1.Close();
                    var end = DateTime.Now;
                    w.Stop();

                    var avr = w.Elapsed.Ticks / count;
                    t1 = new TimeSpan(t1.Ticks / count);
                    t2 = new TimeSpan(t2.Ticks / count);
                    t3 = new TimeSpan(t3.Ticks / count);
                    //Console.WriteLine(string.Format("{0} END: t1={1}, t2={2}, t3={3}", 
                    //  new TimeSpan(avr), t1, t2, t3));
                    Console.WriteLine("{0}, {1} - {2}", Thread.CurrentThread.ManagedThreadId, start.ToString("hh:mm:ss.fff tt"),
                        end.ToString("hh:mm:ss.fff tt"));
                }));

                tasks.Add(t);
            }

            //startEvt.Set();

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