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
        QueryTicketServiceClient _queryTicketMngr;

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
            _queryTicketMngr.QueryNextTicket();
        }

        public void Process(OperatorSession session, OperatorSessionEventMsg msg)
        {
            switch(msg.Event)
            {
                case SessionEventType.ConfirmCall:
                    break;
                case SessionEventType.Logout:
                    _sessionMngr.Logout(session);
                    break;
                case SessionEventType.Query:
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

        void ChangeSessionState(OperatorSession session, OperatorSessionEventMsg msg)
        {
            var status = (Status)msg.OptionalParam;
            _sessionMngr.ChangeState(status, session);
        }
    }
}
