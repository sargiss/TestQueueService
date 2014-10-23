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
        public OperatorWindow Window { get; set; }
        public IEnumerable<AppQueue> Queues { get; set; }
        public string SessionKey { get; }
        public Status Status { get; set; }
    }

    class OperatorWindow
    {
        public long Id { get; set; }
        public int Number { get; set; }
    }

    class AppQueue
    {
        public long Id { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
    }
}
