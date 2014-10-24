using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Queue
{
    public enum TicketStatus
    {
        Unknown = 0,
        /// <summary>
        /// Сформирован и Ожидает вызова
        /// </summary>
        [Display(Name = "Ожидает вызова")]
        Open = 10,

        /// <summary>
        /// Вызывается на прием
        /// </summary>
        [Display(Name = "Вызывается")]
        Call = 20,

        /// <summary>
        /// Вызов пропущен
        /// </summary>
        [Display(Name = "Пропущен")]
        Pass = 30,

        /// <summary>
        /// На приеме
        /// </summary>
        [Display(Name = "На приеме")]
        Process = 40,

        /// <summary>
        /// Отложен
        /// </summary>
        [Display(Name = "Отложен")]
        Pause = 50,

        /// <summary>
        /// Обработан и закрыт
        /// </summary>
        [Display(Name = "Обработан и закрыт")]
        Close = 60
    }
    class TicketStateManager
    {
        public void changeTicketStatus(long ticketId, TicketStatus status, string sessionKey)
        {
            if (ticketId > 0)
            {
                
                
            }
        }

        public void TicketCancel(OperatorSession session)
        {
            if (session != null && session.TicketId > 0)
            {
                var status = (session.Status == SessionStatus.Busy ? TicketStatus.Close : TicketStatus.Open);
                changeTicketStatus(session.TicketId, status, session.SessionKey);
                //LockTicketManager.Instance.Remove(session.TicketId);
            }
        }

        public  void TicketRedirect(string sessionKey, long ticketId)
        {
            
        }

        public void TicketCalled(string sessionKey, long tickerId)
        {
            changeTicketStatus(tickerId, TicketStatus.Call, sessionKey);
        }

        public void TicketPass(string sessionKey, long tickerId)
        {
            changeTicketStatus(tickerId, TicketStatus.Pass, sessionKey);
        }

        public void TicketProcess(string sessionKey, long tickerId)
        {
            changeTicketStatus(tickerId, TicketStatus.Process, sessionKey);
        }

        public void TicketPause(string sessionKey, long tickerId)
        {
            changeTicketStatus(tickerId, TicketStatus.Pause, sessionKey);
        }

        public void TicketClose(string sessionKey, long tickerId)
        {
            changeTicketStatus(tickerId, TicketStatus.Close, sessionKey);
        }
    }
}
