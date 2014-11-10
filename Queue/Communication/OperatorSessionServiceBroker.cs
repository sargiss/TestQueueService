﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;

namespace Queue
{
    class OperatorSessionServiceBroker
    {
        private StopSignal _signal;
        private Dictionary<string, Queue<ZMessageSessioned>> _sessionCmds = 
            new Dictionary<string, Queue<ZMessageSessioned>>();

        public OperatorSessionServiceBroker(StopSignal signal)
        {
            _signal = signal;
        }

        public void Do()
        {
            using (var context = NetMQContext.Create())
            {
                using (NetMQSocket frontend = context.CreateRouterSocket(),
                    backend = context.CreateDealerSocket())
                {
                    frontend.Bind(Config.ServerFrontendAddr);
                    backend.Bind(Config.ServerBackendAddr);

                    frontend.ReceiveReady += (s, e) =>
                    {
                        var msg = new ZMessageSessioned(e.Socket);
                        if (msg.FrameCount != 6)
                        {
                            Console.WriteLine("ERROR!!!");
                        }
                        if (!TryToPushMsg(msg))
                        {
                            PutTask(backend, msg);
                        }
                    };
                    backend.ReceiveReady += (s, e) =>
                    {
                        var msg = new ZMessageSessioned(e.Socket);

                        msg.Send(frontend);

                        var next = TryToPullNextMsg(msg.SessionId);
                        if (next != null)
                        {
                            PutTask(backend, next);
                        }
                    };

                    Poller poller = new Poller(new[] { frontend, backend });

                    Action polling = poller.Start;
                    polling.BeginInvoke(new AsyncCallback(r => { }), null);

                    _signal.WaitForStop();
                    poller.Stop(true);
                }
            }
            Console.WriteLine("Server stopped!");
        }

        private bool TryToPushMsg(ZMessageSessioned cmd)
        {
            if (_sessionCmds.ContainsKey(cmd.SessionId))
            {
                _sessionCmds[cmd.SessionId].Enqueue(cmd);
                return true;
            }
            _sessionCmds[cmd.SessionId] = new Queue<ZMessageSessioned>();
            return false;
        }

        private ZMessageSessioned TryToPullNextMsg(string sessionId)
        {
            if (_sessionCmds[sessionId].Count > 0)
            {
                return _sessionCmds[sessionId].Dequeue();
            }
            _sessionCmds.Remove(sessionId);
            return null;
        }

        private void PutTask(NetMQSocket socket, ZMessage msg)
        {
            msg.Send(socket);
        }

    }
}
