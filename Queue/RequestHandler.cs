using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queue.Communication.Dto;
using Queue.Communication;

namespace Queue
{
    class RequestHandler: IDisposable
    {
        OperatorSessionManager _sessionMngr;
        TicketStateManager _ticketMngr;
        ClientWrapper<SearchTicketServiceClient> _queryService;
        ClientWrapper<TabloServiceClient> _tabloService;

        public RequestHandler(OperatorSessionManager sessionManager, TicketStateManager ticketManager, ConnectionPool connectionPool)
        {
            _sessionMngr = sessionManager;
            _ticketMngr = ticketManager;

            _queryService = connectionPool.GetClient<SearchTicketServiceClient>();
            _tabloService = connectionPool.GetClient<TabloServiceClient>();

            _sessionMngr.TicketClose += _sessionMngr_TicketClose;
            _sessionMngr.SessionDestroy += _sessionMngr_SessionDestroy;
            _sessionMngr.TicketSkipped += _sessionMngr_TicketSkipped;
            _sessionMngr.TicketMissed += _sessionMngr_TicketMissed;
            _sessionMngr.SessionBusy += _sessionMngr_SessionBusy;
            _sessionMngr.SessionFree += _sessionMngr_SessionFree;
        }

        public void Process(OperatorSessionEventMsg msg)
        {
            var session = _sessionMngr.GetSession(msg.SessionId);

            if (session != null)
            {
                switch (msg.Event)
                {
                    case SessionEventType.ConfirmCall:
                        if (session.Status != SessionStatus.Free)
                            throw new IncorrectOperatorAction();
                        _ticketMngr.TicketCalled(session.SessionKey, session.TicketId);
                        _tabloService.Instance.UpdateTablo(session, Communication.Dto.ServiceTicketStatus.Calling);
                        break;
                    case SessionEventType.Logout:
                        _sessionMngr.Logout(session);
                        break;
                    case SessionEventType.AssignTicket:
                        var ticketId = (long)msg.OptionalParam;
                        session.TicketId = ticketId;
                        break;
                    case SessionEventType.RedirectTicket:
                        break;
                    case SessionEventType.SkipTicket:
                        _sessionMngr.SkipTicketByOperator(session);
                        break;
                    case SessionEventType.ChangeState:
                        var status = (SessionStatus)Enum.Parse(typeof(SessionStatus), msg.OptionalParam.ToString());
                        _sessionMngr.ChangeState(status, session);
                        break;
                    default:
                        throw new IncorrectOperatorAction();
                }
            }
            else
            {
                switch(msg.Event)
                {
                    case SessionEventType.AssignTicket:
                        _queryService.Instance.UnassignTicket((long)msg.OptionalParam);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Dispose()
        {
            _queryService.Dispose();
            _tabloService.Dispose();
        }

        void _sessionMngr_SessionFree(OperatorSession obj)
        {
            _queryService.Instance.StartQueringTicket(obj);
        }

        void _sessionMngr_SessionBusy(OperatorSession obj)
        {
            _ticketMngr.TicketProcess(obj.SessionKey, obj.TicketId);
            _tabloService.Instance.UpdateTablo(obj, Communication.Dto.ServiceTicketStatus.Process);
        }

        void _sessionMngr_TicketMissed(OperatorSession obj)
        {
            _tabloService.Instance.UpdateTablo(obj, Communication.Dto.ServiceTicketStatus.Cancel);
        }

        void _sessionMngr_TicketSkipped(OperatorSession obj)
        {
            _ticketMngr.TicketPass(obj.SessionKey, obj.TicketId);
            _tabloService.Instance.UpdateTablo(obj, Communication.Dto.ServiceTicketStatus.Cancel);
        }

        void _sessionMngr_SessionDestroy(OperatorSession obj)
        {
            _ticketMngr.TicketCancel(obj);
            _tabloService.Instance.UpdateTablo(obj, Communication.Dto.ServiceTicketStatus.Cancel);
        }

        void _sessionMngr_TicketClose(OperatorSession obj)
        {
            _ticketMngr.TicketClose(obj.SessionKey, obj.TicketId);
            _tabloService.Instance.UpdateTablo(obj, Communication.Dto.ServiceTicketStatus.Cancel);
        }
    }
}
