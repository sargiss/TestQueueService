using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Queue.Dto;

namespace Queue
{
    class SearchTicketServiceClient: IDisposable
    {
        NetMQContext _context;
        NetMQSocket _client;

        public SearchTicketServiceClient()
        {
            _context = NetMQContext.Create();
            _client = _context.CreateRequestSocket();
            _client.Connect(Config.QueryServerAddr);
        }

        public void StartQueringTicket()
        {
            var query = new QueryTicketMsg();
            Request(query);
        }

        public void FreeTicket()
        {
            var query = new QueryTicketMsg();
            Request(query);
        }

        public void Dispose()
        {
            _client.Dispose();
            _context.Dispose();
        }

        private void Request(QueryTicketMsg query)
        {
            var msg = new ZMessage(query);
            msg.Send(_client);
            msg.Receive(_client);
        }
    }
}
