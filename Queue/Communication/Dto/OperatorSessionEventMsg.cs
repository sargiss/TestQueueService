using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Queue.OperatorSession;

namespace Queue.Dto
{
    class OperatorSessionEventMsg
    {
        public string SessionId { get; set; }
        public SessionEventType Event { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object OptionalParam { get; set; }
    }

}
