using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ;
using Newtonsoft.Json;
using Queue.Communication.Dto;

namespace Queue.Communication
{
    class OperatorSessionServiceClient : IServiceClient
    {
        static long count = 0;

        Encoding _encoding = Encoding.Unicode;
        NetMQContext _context;
        NetMQSocket _client;

        public void Connect()
        {
            count++;
            _context = NetMQContext.Create();
            _client = _context.CreateRequestSocket();
            _client.Connect(Config.BrokerAddr);
            _client.Options.Identity = _encoding.GetBytes("t_" + count + "_" + Thread.CurrentThread.ManagedThreadId.ToString());
            IsConnected = true;
        }

        public void Close()
        {
            _client.Dispose();
            _context.Dispose();
            IsConnected = false;
        }

        public bool IsConnected
        {
            get; private set;
        }

        public void SendOperatorCommand(OperatorSessionEventMsg cmd)
        {
            var zmsg = new ZMessage(JsonConvert.SerializeObject(cmd));
            zmsg.PushStr(CommunicationPrimitives.ClientTypeId);
            zmsg.Send(_client);

            var answer = new ZMessage(_client);
        }
    }
}