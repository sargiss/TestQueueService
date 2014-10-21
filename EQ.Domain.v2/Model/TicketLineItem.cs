using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Строка в талоне
    /// </summary>
    public class TicketLineItem
    {
        public virtual long Id { get; protected set; }
        /// <summary>
        /// Введеное значение пользователем
        /// </summary>
        public virtual string Value { get; protected set; }

        /// <summary>
        /// Номер строки
        /// </summary>
        public virtual LineSetting LineSetting { get; protected set; }

		protected Ticket Ticket { get; private set; }

        public TicketLineItem()
        { }

        internal TicketLineItem(Ticket ticket, LineSetting linesetting, string value)
        {
			Ticket = ticket;
            LineSetting = linesetting;
            Value = value; 
        }

        public virtual TicketLineItem Clone(Ticket ticket)
        {
            return new TicketLineItem
            {
                Value = this.Value,
                LineSetting = this.LineSetting,
                Ticket = ticket
            };
        }
    }

}
