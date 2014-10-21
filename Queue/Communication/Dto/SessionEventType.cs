using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Dto
{
    public enum SessionEventType
    {
        ChangeState,
        SkipTicket,
        RedirectTicket,
        Query,
        Logout,
        ConfirmCall
    }
}
