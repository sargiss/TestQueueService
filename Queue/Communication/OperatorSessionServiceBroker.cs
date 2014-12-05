using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;
using System.Diagnostics;
using Queue.Common;

namespace Queue.Communication
{
    class OperatorSessionServiceBroker
    {
        private StopSignal _signal;
        private HashSet<byte[]> _workers = new HashSet<byte[]>();
        private List<byte[]> _waitingWorkers = new List<byte[]>();
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
                        //Console.Write(string.Join("\n", zmsg.AllData));

                        var adr = zmsg.Pop();
                        var address = Encoding.Unicode.GetString(adr); // zmsg.PopStr();

                        if (address[0] == 't')
                        {
                            Debug.Write("adr: ");
                            for (var b = 0; b < adr.Length; b++ )
                                Debug.Write("_" + adr[b]);
                            Debug.WriteLine("");
                        }
                        if (address[0] != 't' && address[0] != 'w')
                        {
                            Console.WriteLine("ERROR ADDR!!!");
                        }

                        var empty = zmsg.PopStr();
                        var typeOfRequester = zmsg.PopStr();

                        switch (CommunicationPrimitives.GetTypeOfClient(typeOfRequester))
                        {
                            case CommunicationPrimitives.TypeOfClient.Client:
                                ProcessClient(adr, zmsg);
                                break;
                            case CommunicationPrimitives.TypeOfClient.Worker:
                                ProcessWorker(adr, zmsg);
                                break;
                            case CommunicationPrimitives.TypeOfClient.TaskManager:
                                ProcessTaskManager(adr, zmsg);
                                break;
                            default:
                                Console.WriteLine("Incorrect type of requestor.");
                                break;
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

        private void ProcessClient(byte[] sender, ZMessage zmsg)
        {
            Console.WriteLine("> ProcessClient");
            zmsg.Wrap(sender, new byte[0]);
            _requests.Enqueue(zmsg);
            //System.Diagnostics.Debug.WriteLine("sender: " + sender);
            Dispatch();
        }

        private void ProcessTaskManager(byte[] address, ZMessage zmsg)
        {
            var syncReply = new ZMessage(ReplyStatus.OK);
            syncReply.Push(address);
            syncReply.Send(_broker);
            _requests.Enqueue(zmsg);
            Dispatch();
        }

        private void ProcessWorker(byte[] address, ZMessage zmsg)
        {
            Console.WriteLine("> ProcessWorker");

            var workerExists = _workers.Contains(address, new ByteArrayComparer());

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
                        if (zmsg.PopStr() != ReplyStatus.QUEUED)
                        {
                            var clientAddr = zmsg.Unwrap();
                            zmsg.Push(CommunicationPrimitives.ClientTypeId);
                            zmsg.Wrap(clientAddr, new byte[0]);
                            zmsg.Send(_broker);
                        }
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

        private void WaitingWorker(byte[] address)
        {
            _waitingWorkers.Add(address);
            Dispatch();
        }

        private void DeleteWorker(byte[] address, bool disconnect)
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

        private void SendToWorker(byte[] address, string cmd, ZMessage zmsg)
        {
            if (zmsg == null)
                zmsg = new ZMessage();
            zmsg.Push(cmd);
            zmsg.Push(CommunicationPrimitives.WorkerTypeId);
            zmsg.Push(string.Empty);
            zmsg.Push(address);
            zmsg.Send(_broker);
        }
    }
}
