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
    class WorkerApi: IServiceClient
    {
        NetMQSocket _worker;
        NetMQContext _context;
        byte[] _replyTo;

        public WorkerApi(ContextFactory factory)
        {
            _context = factory.GetContext();
        }

        NetMQSocket ReconnectToBroker(NetMQContext ctx)
        {
            if (_worker != null)
            {
                _worker.Dispose();
            }
            _worker = ctx.CreateDealerSocket();
            _worker.Options.Identity = Encoding.Unicode.GetBytes("w_t_" + Thread.CurrentThread.ManagedThreadId);
            _worker.Connect(Config.BrokerAddr);

            SendToBroker(CommunicationPrimitives.Commands.READY, null);

            return _worker;
        }

        private void SendToBroker(string cmd, ZMessage zmsg)
        {
            if (zmsg == null)
                zmsg = new ZMessage();
            zmsg.Push(cmd);
            zmsg.Push(CommunicationPrimitives.WorkerTypeId);
            zmsg.Push(string.Empty);
            zmsg.Send(_worker);
        }

        public ZMessage Recieve()
        {
            var msg = new ZMessage(_worker);

            var empty = msg.Pop();
            var typeOfRequest = msg.PopStr();
            var cmd = msg.PopStr();
            if (cmd == CommunicationPrimitives.Commands.DISCONNECT)
                ReconnectToBroker(_context);
            else
            {
                _replyTo = msg.Unwrap();
                Console.WriteLine(">HandleRequest");
                return msg;
            }
            return null;
        }

        public void Send(string answerStatus, ZMessage answer = null)
        {
            if (answer == null)
                answer = new ZMessage();

            answer.Wrap(_replyTo, new byte[0]);
            answer.Push(answerStatus);
            SendToBroker(CommunicationPrimitives.Commands.REPLY, answer);
        }

        public void Connect()
        {
            ReconnectToBroker(_context);
        }

        public void Close()
        {
            _worker.Close();
        }

        public bool IsConnected
        {
            get { throw new NotImplementedException(); }
        }
    }
}
