using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;
using Queue.Communication.Dto;
using System.Diagnostics;

namespace Queue.Communication
{
    class BrokerClient : ClientBase, IServiceClient
    {
        string _typeId;

        public BrokerClient(IRequestHandler handler, string clientTypeId)
        {
            _handler = handler;
            _typeId = clientTypeId;
            Connect();
        }

        public void Connect()
        {
                _client = _context.CreateRequestSocket();
                _client.Connect(Config.BrokerAddr);
                //_client.Options.Identity = _encoding.GetBytes("t_" + count + "_" + Thread.CurrentThread.ManagedThreadId.ToString());
                _client.ReceiveReady += _client_ReceiveReady; ;
           

                /*Debug.Write("identity: ");
                for (var b = 0; b < _client.Options.Identity.Length; b++)
                    Debug.Write("_" + _client.Options.Identity[b]);
                Debug.WriteLine("");*/
        }

        void _client_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var answer = new ZMessage(_client);
            Console.WriteLine("Answer:");
            Console.WriteLine(answer.ToString());
        }

        public void Close()
        {
            _client.Dispose();
            //_context.Dispose();
            IsConnected = false;
        }

        public bool IsConnected
        {
            get; private set;
        }

        public void SendToBroker(ZMessage zmsg)
        {
            zmsg.Push(_typeId);
            zmsg.Send(_client);

            _client.Poll();
        }

        public void SendOperatorCommand(OperatorSessionEventMsg cmd)
        {
            var data = JsonConvert.SerializeObject(cmd);
            var zmsg = new ZMessage(data);
            SendToBroker(zmsg);
        }
    }
}