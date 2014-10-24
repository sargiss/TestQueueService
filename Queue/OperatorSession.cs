using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    public class OperatorSession
    {
        public long TicketId { get; set; }
        public OperatorWindow Window { get; set; }
        public IEnumerable<AppQueue> Queues { get; set; }
        public string SessionKey { get; set; }
        public SessionStatus Status { get; set; }
    }

    public class OperatorWindow
    {
        public long Id { get; set; }
        public string Number { get; set; }
    }

    public class AppQueue
    {
        public long Id { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
    }
}
