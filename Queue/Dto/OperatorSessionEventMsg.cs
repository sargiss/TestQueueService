using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Queue.Communication.Dto
{
    public class OperatorSessionEventMsg
    {
        public string SessionId { get; set; }
        public SessionEventType Event { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object OptionalParam { get; set; }
    }

}
