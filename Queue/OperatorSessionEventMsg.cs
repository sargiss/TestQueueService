using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Queue
{
    class OperatorSessionEventMsg
    {
        public string SessionId { get; set; }
        public SessionEvent Event { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Param { get; set; }
    }

    public enum SessionEvent
    {
        StateChanged,
        SkipTicket,
        RedirectTicket,
        Pause,
        Query,
        Logout,
        ConfirmCall
    }
}
