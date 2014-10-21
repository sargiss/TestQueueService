using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace Queue
{
    class QueryTicketServer
    {
        private IQueryTicketManager _queryMngr;

        public void Do()
        {
            using(var context = NetMQContext.Create())
            {
                using(NetMQSocket subscriber = context.CreatePullSocket(),
                    publisher = context.CreatePublisherSocket())
                {
                    while(true)
                    {

                    }
                }
            }
        }
    }
}
