using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    static class CommunicationPrimitives
    {
        public static string ClientTypeId
        {
            get { return "client_id"; }
        }

        public static string WorkerTypeId
        {
            get { return "worker_id"; }
        }

        public static bool IsClient(string id)
        {
            return string.Compare(ClientTypeId, id) == 0;
        }

        public static bool IsWorker(string id)
        {
            return string.Compare(WorkerTypeId, id) == 0;
        }

        public class Commands
        {
            public const string REPLY = "reply_cmd";
            public const string READY = "ready_cmd";
            public const string REQUEST = "request_cmd";
            public const string DISCONNECT = "disconnect_cmd";
        }

    }
}
