using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    interface IHandlerCommand
    {
        void Execute();
        void Cancel();
    }
}
