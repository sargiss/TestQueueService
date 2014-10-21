using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// История обработки талонов
    /// </summary>
    public class Process
    {
        public virtual long Id { get; protected set; }
        /// <summary>
        /// Дата и время изменения статуса
        /// </summary>
        public virtual DateTime Date { get; protected set; }

        /// <summary>
        /// Талонs
        /// </summary>
        public virtual Ticket Ticket { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Окно
        /// </summary>
        public virtual Window Window { get; set; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public virtual string Remark { get; set; }

        /// <summary>
        /// Статус талона
        /// </summary>
        public virtual TicketStatus Status { get; set; }

        public Process()
        {
            Date = DateTime.Now;
        }

    }

}
