using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    interface ISearchTicketManager
    {
        long QueryTicket(OperatorSession session);
        void LockTicket(long ticketId);
        void ReleaseTicket(long ticketId);
    }
}
