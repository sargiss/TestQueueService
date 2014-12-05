using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    interface IServiceClient
    {
        void Connect();
        void Close();
    }
}
