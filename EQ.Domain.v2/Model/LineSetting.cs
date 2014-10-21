using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Настройка строки для живой очереди
    /// </summary>
    public class LineSetting
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Наименование поля
        /// </summary>
        public virtual string Caption { get; set; }

        /// <summary>
        /// Требовать обязательного заполнения поля
        /// </summary>
        public virtual bool Require { get; set; }

        /// <summary>
        /// Регулярное выражение
        /// </summary>
        public virtual string Regex { get; set; }

        /// <summary>
        /// Количество строк (в интерфейсе)
        /// </summary>
        public virtual int LineCount { get; set; }

        /// <summary>
        /// Код поля для портальной очереди
        /// </summary>
        public virtual string PortalCode { get; set; }

		public virtual Queue Queue { get; protected set; }

	    protected LineSetting()
        { }

        public LineSetting(Queue queue)
        {
			Queue = queue;
        }
    }

}
