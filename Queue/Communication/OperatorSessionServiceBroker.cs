using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;

namespace Queue.Communication
{
    class OperatorSessionServiceBroker
    {
        private StopSignal _signal;
        private HashSet<string> _workers = new HashSet<string>();
        private List<string> _waitingWorkers = new List<string>();
        private NetMQSocket _broker;
        private Queue<ZMessage> _requests = new Queue<ZMessage>();

        public OperatorSessionServiceBroker(StopSignal signal)
        {
            _signal = signal;
        }

        public void Do()
        {
            using (var context = NetMQContext.Create())
            {
                using (_broker = context.CreateRouterSocket())
                {
                    _broker.Bind(Config.BrokerAddr);

                    _broker.ReceiveReady += (s, e) =>
                    {
                        var zmsg = new ZMessage(e.Socket);

                        var address = zmsg.PopStr();
                        var empty = zmsg.PopStr();
                        var typeOfRequester = zmsg.PopStr();

                        if (CommunicationPrimitives.IsClient(typeOfRequester))
                        {
                            ProcessClient(address, zmsg);
                        }
                        else
                        {
                            if (CommunicationPrimitives.IsWorker(typeOfRequester))
                            {
                                ProcessWorker(address, zmsg);
                            }
                            else
                            {
                                Console.WriteLine("Incorrect type of requestor.");
                            }
                        }
                    };

                    Poller poller = new Poller(new[] { _broker });

                    Action polling = poller.Start;
                    polling.BeginInvoke(new AsyncCallback(r => { }), null);

                    _signal.WaitForStop();
                    poller.Stop(true);
                }
            }
            Console.WriteLine("Server stopped!");
        }

        private void ProcessClient(string sender, ZMessage zmsg)
        {
            _requests.Enqueue(zmsg);
            Dispatch();
        }

        private void ProcessWorker(string address, ZMessage zmsg)
        {
            var workerExists = _workers.Contains(address);

            var cmd = zmsg.PopStr();

            switch (cmd)
            {
                case CommunicationPrimitives.Commands.READY:
                    if (workerExists)
                    {
                        DeleteWorker(address, true);
                    }
                    else
                    {
                        _workers.Add(address);
                        WaitingWorker(address);
                    }
                    break;
                case CommunicationPrimitives.Commands.REPLY:
                    if (workerExists)
                    {
                        zmsg.PushStr(CommunicationPrimitives.ClientTypeId);
                        zmsg.Send(_broker);
                        WaitingWorker(address);
                    }
                    else
                    {
                        DeleteWorker(address, true);
                    }
                    break;
                case CommunicationPrimitives.Commands.DISCONNECT:
                    DeleteWorker(address, false);
                    break;
                default:
                    Console.WriteLine("Incorrect command:" + cmd);
                    break;
            }
        }

        private void WaitingWorker(string address)
        {
            _waitingWorkers.Add(address);
            Dispatch();
        }

        private void DeleteWorker(string address, bool disconnect)
        {
            if (disconnect)
            {
                SendToWorker(address, CommunicationPrimitives.Commands.DISCONNECT, null);
            }
            _workers.Remove(address);
            _waitingWorkers.Remove(address);
        }

        private void Dispatch()
        {
            while(_waitingWorkers.Count > 0 && _requests.Count > 0)
            {
                var zmsg = _requests.Dequeue();
                var address = _waitingWorkers[0];
                _waitingWorkers.RemoveAt(0);
                SendToWorker(address, CommunicationPrimitives.Commands.REQUEST, zmsg);
            }
        }

        private void SendToWorker(string address, string cmd, ZMessage zmsg)
        {
            if (zmsg == null)
                zmsg = new ZMessage();
            zmsg.PushStr(cmd);
            zmsg.PushStr(CommunicationPrimitives.WorkerTypeId);
            zmsg.PushStr(string.Empty);
            zmsg.PushStr(address);
            zmsg.Send(_broker);
        }
    }
}
