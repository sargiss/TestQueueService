using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    class DefaultMessageHandler: IRequestHandler
    {
        public ZMessage Process(ZMessage zmsg)
        {
            Console.WriteLine(zmsg.AllData);
            return zmsg;
        }
    }
}
