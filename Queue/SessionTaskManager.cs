using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Communication;

namespace Queue
{
    class SessionTaskManager: IRequestHandler
    {
        Dictionary<string, Queue<ZMessage>> _requestsPerSession = new Dictionary<string, Queue<ZMessage>>();
        HashSet<string> _processedSessions = new HashSet<string>();
        BrokerClient _brokerClient = new BrokerClient(null, CommunicationPrimitives.TaskManagerTypeId);

        public ZMessage Process(ZMessage zmsg)
        {
            var reply = new ZMessage();
            var action = zmsg.PopStr();
            var sessionKey = zmsg.PopStr();
            switch (action)
            {
                case RequestStatus.PUT_SESSION_TASK:
                    if (AddTaskToProcessed(sessionKey, zmsg))
                        reply.Push(ReplyStatus.OK);
                    else
                        reply.Push(ReplyStatus.QUEUED);
                    break;
                case RequestStatus.DEL_SESSION_TASK:
                    var task = DeleteSessionTask(sessionKey);
                    if (task != null)
                    {
                        SendToBroker(task);
                    }
                    reply.Push(ReplyStatus.OK);
                    break;
                default: throw new Exception("Invalid operation " + action);
            }
            return reply;
        }

        void SendToBroker(ZMessage zmsg)
        {
            _brokerClient.SendToBroker(zmsg);
        }

        bool AddTaskToProcessed(string sessionKey, ZMessage zmsg)
        {
            if (_processedSessions.Contains(sessionKey))
            {
                if (!_requestsPerSession.ContainsKey(sessionKey))
                {
                    _requestsPerSession[sessionKey] = new Queue<ZMessage>();
                }
                _requestsPerSession[sessionKey].Enqueue(zmsg);
                return false;
            }
            _processedSessions.Add(sessionKey);
            return true;
        }

        ZMessage DeleteSessionTask(string sessionKey)
        {
            _processedSessions.Remove(sessionKey);
            if (_requestsPerSession.ContainsKey(sessionKey) && _requestsPerSession[sessionKey].Count > 0)
            {
                return _requestsPerSession[sessionKey].Dequeue();
            }
            return null;
        }
    }
}
