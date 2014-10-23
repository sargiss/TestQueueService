using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Newtonsoft.Json;
using Queue.Dto;
using System.Threading;

namespace Queue
{
    class SearchTicketServiceWorker
    {
        LinkedList<Timer> timers = new LinkedList<Timer>();

        private ISearchTicketManager _queryMngr;
        OperatorSessionManager _sessionMngr;

        public void Do()
        {
            using(var context = NetMQContext.Create())
            {
                using(NetMQSocket server = context.CreateResponseSocket(),
                    client = context.CreateRequestSocket())
                {
                    server.Bind(Config.QueryServerAddr);
                    client.Connect(Config.ServerFrontendAddr);

                    while(true)
                    {
                        var msg = new ZMessage(server);
                        server.SendMore(string.Empty);
                        server.Send("OK");

                        var queryMsg = JsonConvert.DeserializeObject<QueryTicketMsg>(msg.BodyToString());
                        Process(queryMsg.SessionKey);
                    }
                }
            }
        }

        private void Process(string sessionKey)
        {
            var session = _sessionMngr.GetSession(sessionKey);
            if (session != null)
            {
                var ticketId = _queryMngr.QueryTicket(session);
                if (ticketId > 0)
                {
                    _queryMngr.LockTicket(ticketId);
                }
                else
                {
                    Timer t = null;
                    t = new Timer(new TimerCallback(o =>
                     {
                         lock (timers)
                         {
                             timers.Remove(t);
                         }
                         using (var client = new SearchTicketServiceClient())
                         {
                             client.StartQueringTicket();
                         }
                     }), null, 2000, Timeout.Infinite);
                    timers.AddLast(t);
                }
            }
        }
    }
}
