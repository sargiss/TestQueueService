using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue
{
    class Config
    {
        public static string BrokerAddr = "tcp://localhost:5565";
        public static string BrokerPollAddr = "tcp://localhost:5564";
        public static string ServerBackendAddr = "tcp://localhost:5566";
        public static string ServerFrontendAddr = "tcp://localhost:5567";
        public static string QueryServerAddr = "tcp://localhost:5568";
        public static string TabloServerAddr = "tcp://localhost:5569";
        public static string TaskManagerAddr = "tcp://localhost:6661";
    }
}
