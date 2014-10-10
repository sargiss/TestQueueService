using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EQ.Core.Operator;

namespace Queue
{
    class OperatorSessionManager
    {
        Dictionary<string, OperatorSession> _sessions = new Dictionary<string, OperatorSession>();

        public event Action SessionFree;
        public event Action SessionClear;
        public event Action SessionBusy;
        public event Action SessionDestroy;

        public static OperatorSessionManager Instance { get; }

        public string Login(string user, string password)
        {
            var key = Guid.NewGuid().ToString();
            _sessions.Add(key, new OperatorSession());
            return key;
        }

        public OperatorSession GetSession(string SessionKey)
        {
            return null;
        }

        public void Logout(string sessionKey)
        {
            _sessions.Remove(sessionKey);
        }

        public void ChangeState(Status status, OperatorSession sessionInfo)
        {
            var currentStatus = sessionInfo.SessionStatus;
            if (status == currentStatus)
                return;

            switch (status)
            {
                case Status.Free:
                    sessionInfo.SessionStatus = status;
                    if (currentStatus == Status.Busy)
                    {
                        
                        SessionClear();
                        SessionFree();
                    }
                    else
                    {
                        SessionFree();
                    }
                    break;
                case Status.Busy:
                    if (currentStatus == Status.Free)
                    {
                        sessionInfo.SessionStatus = status;
                        SessionBusy();
                    }
                    else
                    {
                        throw new Exception("Incorrect status");
                    }
                    break;
                case Status.Pause:
                    sessionInfo.SessionStatus = status;
                    if (currentStatus == Status.Free)
                    {
                        SessionClear();
                    }
                    else
                    {

                    }
                    break;
                default:
                    throw new Exception("Incorrect status");
            }
        }
    }

    class OperatorSession
    {
        public long TicketId { get; set; }
        public long WindowId { get; set; }
        public string SessionKey { get; }
        public Status SessionStatus { get; set; }
    }
}
