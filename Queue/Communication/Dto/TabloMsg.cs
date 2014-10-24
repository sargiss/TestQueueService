using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication.Dto
{
    class TabloMsg
    {
        public string SessionKey { get; set; }
        public long TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string WindowNumber { get; set; }
        public ServiceTicketStatus Status { get; set; }
    }
}
