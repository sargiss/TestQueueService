using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Dto;
using EQ.Core.Operator;

namespace Queue
{
    class RequestHandler
    {
        OperatorSessionManager _sessionMngr;
        TicketStateManager _ticketMngr;
        SearchTicketServiceClient _queryTicketMngr;

        public RequestHandler()
        {
            _sessionMngr.TicketClose += () => _ticketMngr.TicketClose(null, 0);
            _sessionMngr.SessionDestroy += () => _ticketMngr.TicketCancel(null);
            _sessionMngr.TicketSkipped += () => _ticketMngr.TicketPass(null, 0);
            _sessionMngr.TicketMissed += () => { };
            _sessionMngr.SessionBusy += () => _ticketMngr.TicketProcess(null, 0);
            _sessionMngr.SessionFree += _sessionMngr_SessionFree;
        }

        void _sessionMngr_SessionFree()
        {
            _queryTicketMngr.StartQueringTicket();
        }

        public void Process(OperatorSessionEventMsg msg)
        {
            var session = OperatorSessionManager.Instance.GetSession(msg.SessionId);

            if (session != null)
            {
                switch (msg.Event)
                {
                    case SessionEventType.ConfirmCall:
                        break;
                    case SessionEventType.Logout:
                        _sessionMngr.Logout(session);
                        break;
                    case SessionEventType.Query:
                        _queryTicketMngr.StartQueringTicket();
                        break;
                    case SessionEventType.RedirectTicket:
                        break;
                    case SessionEventType.SkipTicket:
                        _sessionMngr.SkipTicketByOperator(session);
                        break;
                    case SessionEventType.ChangeState:
                        ChangeSessionState(session, msg);
                        break;
                    default:
                        throw new Exception();
                }
            }
            else
            {
                switch(msg.Event)
                {
                    case SessionEventType.Query:
                        _queryTicketMngr.FreeTicket();
                        break;
                    default:
                        break;
                }
            }
        }

        void ChangeSessionState(OperatorSession session, OperatorSessionEventMsg msg)
        {
            var status = (Status)msg.OptionalParam;
            _sessionMngr.ChangeState(status, session);
        }

    }
}
