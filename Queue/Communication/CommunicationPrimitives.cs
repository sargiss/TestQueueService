using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    static class CommunicationPrimitives
    {
        static Dictionary<string, TypeOfClient> map = new Dictionary<string, TypeOfClient>{
            {ClientTypeId, TypeOfClient.Client},
            {WorkerTypeId, TypeOfClient.Worker},
            {TaskManagerTypeId, TypeOfClient.TaskManager}
        };

        public static string ClientTypeId
        {
            get { return "client_id"; }
        }

        public static string WorkerTypeId
        {
            get { return "worker_id"; }
        }

        public static string TaskManagerTypeId
        {
            get { return "tm_client_id"; }
        }

        public static TypeOfClient GetTypeOfClient(string id)
        {
            return map[id];
        }

        public class Commands
        {
            public const string REPLY = "reply_cmd";
            public const string READY = "ready_cmd";
            public const string REQUEST = "request_cmd";
            public const string DISCONNECT = "disconnect_cmd";
        }

        public enum TypeOfClient
        {
            Client,
            Worker,
            TaskManager
        }
    }
}
