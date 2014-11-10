using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Queue.Communication.Dto;

namespace Queue.Communication
{
    class SearchTicketServiceClient: IServiceClient
    {
        NetMQContext _context;
        NetMQSocket _client;

        public void StartQueringTicket(OperatorSession session)
        {
            var query = new QueryTicketMsg()
            {
                SessionKey = session.SessionKey
            };
            Request(query);
        }

        public void UnassignTicket(long ticketId)
        {
            var query = new QueryTicketMsg()
                {
                    TicketId = ticketId
                };
            Request(query);
        }

        private void Request(QueryTicketMsg query)
        {


            var msg = new ZMessage(query);
            //msg.Send(_client);
            //_client.SendMore(string.Empty);
            //_client.Send("fuck");
        }

        public void Connect()
        {
            _context = NetMQContext.Create();
            _client = _context.CreatePublisherSocket();
            _client.Connect(Config.QueryServerAddr);
            IsConnected = true;
        }

        public void Close()
        {
            _client.Dispose();
            _context.Dispose();
        }

        public bool IsConnected
        {
            get;
            private set;
        }
    }
}
