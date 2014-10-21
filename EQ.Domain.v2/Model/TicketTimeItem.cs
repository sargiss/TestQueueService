using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{

    /// <summary>
    /// Дополнительный параметр расчета времени
    /// </summary>
    public class TicketTimeItem
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Введеное значение пользователем
        /// </summary>
        public virtual int Value { get; protected set; }

        /// <summary>
        /// Дополнительное время в секундах
        /// </summary>
        public virtual int AddtionTime { get; protected set; }

        /// <summary>
        /// Номер дополнительного параметра
        /// </summary>
        public virtual TimeSetting TimeSetting { get; protected set; }

		protected virtual Ticket Ticket { get; private set; }

        protected TicketTimeItem()
        { }

        internal TicketTimeItem(Ticket ticket, TimeSetting timesetting, int value)
        {
            AddtionTime = 0;
			Ticket = ticket;
            Value = value;
            TimeSetting = timesetting;

            if (TimeSetting != null)
                AddtionTime = TimeSetting.CalculateTime(Value);
        }
    }

}
