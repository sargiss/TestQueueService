using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication.Dto
{
    class QueryTicketMsg
    {
        public string SessionKey { get; set; }
        public long TicketId { get; set; }
    }
}
