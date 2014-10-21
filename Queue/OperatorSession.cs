using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    class OperatorSession
    {
        public long TicketId { get; set; }
        public long WindowId { get; set; }
        public string SessionKey { get; }
        public Status Status { get; set; }
    }
}
