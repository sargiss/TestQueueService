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

        public event Action TicketClose;
        public event Action SessionFree;
        public event Action SessionPause;
        public event Action SessionBusy;
        public event Action SessionDestroy;
        public event Action TicketSkipped;
        public event Action TicketMissed;

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

        public void Logout(OperatorSession session)
        {
            _sessions.Remove(session.SessionKey);
            SessionDestroy();
        }

        public void ChangeState(Status status, OperatorSession sessionInfo)
        {
            var currentStatus = sessionInfo.Status;
            if (status == currentStatus)
                return;

            switch (status)
            {
                case Status.Free:
                    if (currentStatus == Status.Busy)
                    {
                        TicketClose();
                    }
                    SessionFree();
                    break;
                case Status.Busy:
                    if (currentStatus == Status.Free)
                    {
                        SessionBusy();
                    }
                    else
                    {
                        throw new Exception("Incorrect status");
                    }
                    break;
                case Status.Pause:
                    if (currentStatus == Status.Busy)
                    {
                        TicketClose();
                    }
                    SessionPause();
                    break;
                default:
                    throw new Exception("Incorrect status");
            }

            sessionInfo.Status = status;
        }

        public void SkipTicketByOperator(OperatorSession session)
        {
            TicketSkipped();
            SessionFree();
        }

        public void MissedByCustomer(OperatorSession session)
        {
            TicketMissed();
            SessionFree();
        }
    }

    class OperatorSession
    {
        public long TicketId { get; set; }
        public long WindowId { get; set; }
        public string SessionKey { get; }
        public Status Status { get; set; }
    }
}
