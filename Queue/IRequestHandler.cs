using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Communication;

namespace Queue
{
    interface IRequestHandler
    {
        ZMessage Process(ZMessage zmsg);
    }
}
