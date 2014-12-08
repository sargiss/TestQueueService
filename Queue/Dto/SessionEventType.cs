using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication.Dto
{
    public enum SessionEventType
    {
        ChangeState,
        SkipTicket,
        RedirectTicket,
        AssignTicket,
        Logout,
        ConfirmCall
    }
}
