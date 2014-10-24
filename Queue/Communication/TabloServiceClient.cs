using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Queue.Communication.Dto;

namespace Queue.Communication
{
    public class TabloServiceClient: IServiceClient
    {
        Encoding _encoding = Encoding.Unicode;
        NetMQContext _context;
        NetMQSocket _client;

        public void Connect()
        {
            _context = NetMQContext.Create();
            _client = _context.CreatePublisherSocket();
            _client.Connect(Config.TabloServerAddr);
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
            get;
            private set;
        }

        public void UpdateTablo(OperatorSession session, ServiceTicketStatus status)
        {
            var msg = new ZMessage(new TabloMsg()
                {
                    SessionKey = session.SessionKey,
                    TicketId = session.TicketId,
                    TicketNumber = "",
                    WindowNumber = session.Window.Number,
                    Status = status
                });
            msg.Send(_client);
        }
    }
}
