using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Queue.Communication;
using Newtonsoft.Json;
using Queue.Communication.Dto;

namespace Queue
{
    class OperatorSessionRequestHandler: IRequestHandler
    {
        WorkerClient _client = new WorkerClient();

        public ZMessage Process(ZMessage zmsg)
        {
            var body = zmsg.BodyToString();
            var sessionEvt = JsonConvert.DeserializeObject<OperatorSessionEventMsg>(body);

            zmsg.Push();
        }
    }
}
