using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Queue
{
    class OperatorSessionManager
    {
        ConcurrentDictionary<string, OperatorSession> _sessions = new ConcurrentDictionary<string, OperatorSession>();

        public event Action<OperatorSession> TicketClose;
        public event Action<OperatorSession> SessionFree;
        public event Action<OperatorSession> SessionPause;
        public event Action<OperatorSession> SessionBusy;
        public event Action<OperatorSession> SessionDestroy;
        public event Action<OperatorSession> TicketSkipped;
        public event Action<OperatorSession> TicketMissed;

        public OperatorSessionManager()
        {
            SessionFree += OperatorSessionManager_SessionFree;
            SessionPause += OperatorSessionManager_SessionFree;
        }

        void OperatorSessionManager_SessionFree(OperatorSession obj)
        {
            obj.TicketId = 0;
        }

        public string Login(string user, string password)
        {
            var key = Guid.NewGuid().ToString();
            _sessions.TryAdd(key, new OperatorSession()
                {
                    SessionKey = key,
                    Status = SessionStatus.NoDefined,
                    TicketId = 0,
                    Window = new OperatorWindow()
                    {
                        Id = 1,
                        Number = user
                    }
                });
            return key;
        }

        public OperatorSession GetSession(string sessionKey, bool throwException = false)
        {
            if (!_sessions.ContainsKey(sessionKey))
            {
                if (throwException)
                    throw new Exception("Can not find session specified by sessionKey=" + sessionKey);
                else
                    return null;
            }
            return _sessions[sessionKey];
        }

        public void Logout(OperatorSession session)
        {
            OperatorSessionManager_SessionFree(session);
            _sessions.TryRemove(session.SessionKey, out session);
            //Console.WriteLine("Remove: " + session.SessionKey);
            SessionDestroy(session);
        }

        public void ChangeState(SessionStatus status, OperatorSession sessionInfo)
        {
            var currentStatus = sessionInfo.Status;
            if (status == currentStatus)
                return;

            if (currentStatus == SessionStatus.NoDefined && status != SessionStatus.Free)
                throw new IncorrectOperatorAction();

            switch (status)
            {
                case SessionStatus.Free:
                    if (currentStatus == SessionStatus.Busy)
                    {
                        TicketClose(sessionInfo);
                    }
                    SessionFree(sessionInfo);
                    break;
                case SessionStatus.Busy:
                    if (currentStatus == SessionStatus.Free)
                    {
                        SessionBusy(sessionInfo);
                    }
                    else
                    {
                        throw new IncorrectOperatorAction();
                    }
                    break;
                case SessionStatus.Pause:
                    if (currentStatus == SessionStatus.Busy)
                    {
                        TicketClose(sessionInfo);
                    }
                    SessionPause(sessionInfo);
                    break;
                default:
                    throw new Exception("Incorrect status");
            }

            sessionInfo.Status = status;
        }

        public void SkipTicketByOperator(OperatorSession session)
        {
            if (session.Status != SessionStatus.Free)
                throw new IncorrectOperatorAction();
            TicketSkipped(session);
            SessionFree(session);
        }

        public void MissedByCustomer(OperatorSession session)
        {
            if (session.Status != SessionStatus.Free)
                throw new IncorrectOperatorAction();
            TicketMissed(session);
            SessionFree(session);
        }
    }
}
