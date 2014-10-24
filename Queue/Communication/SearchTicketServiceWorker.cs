using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Newtonsoft.Json;
using Queue.Communication.Dto;
using System.Threading;

namespace Queue.Communication
{
    class SearchTicketServiceWorker
    {
        LinkedList<Timer> timers = new LinkedList<Timer>();

        private ISearchTicketManager _queryMngr;
        OperatorSessionManager _sessionMngr;
        ConnectionPool _connectionPool;

        public void Do()
        {
            using(var context = NetMQContext.Create())
            {
                using(NetMQSocket server = context.CreateSubscriberSocket())
                {
                    server.Bind(Config.QueryServerAddr);

                    while(true)
                    {
                        var msg = new ZMessage(server);

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
                    using(var client = _connectionPool.GetClient<OperatorSessionServiceClient>())
                    {
                        client.Instance.SendOperatorCommand(new OperatorSessionEventMsg()
                        {
                            SessionId = sessionKey,
                            Event = SessionEventType.AssignTicket,
                            OptionalParam = ticketId
                        });
                    }
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
                         using (var client = _connectionPool.GetClient<SearchTicketServiceClient>())
                         {
                             client.Instance.StartQueringTicket();
                         }
                     }), null, 2000, Timeout.Infinite);
                    lock (timers)
                    {
                        timers.AddLast(t);
                    }
                }
            }
        }
    }
}
