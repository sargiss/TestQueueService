using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Параметр расчета времени для очереди
    /// </summary>
    public class TimeSetting
    {
        public virtual long Id { get; protected set; }

		public virtual Queue Queue { get; protected set; }

        /// <summary>
        /// Наименование параметра
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Минимальное значение параметра
        /// </summary>
        public virtual int Minimal { get; set; }

        /// <summary>
        /// Максимальное значение параметра
        /// </summary>
        public virtual int Maximum { get; set; }

        /// <summary>
        /// Установленное по умолчанию значение параметра
        /// </summary>
        public virtual int Normal { get; set; }

        /// <summary>
        /// Код параметра для портальной очереди
        /// </summary>
        public virtual string PortalCode { get; set; }

        /// <summary>
        /// Дополнительное время (в секундах) за максимальное значение параметра
        /// </summary>
        public virtual int AddtionTime { get; set; }

        public virtual int CalculateTime(int value)
        {
            double rate = 0;
            if (Maximum <= Minimal || Maximum == 0 || AddtionTime == 0)
                return 0;
            rate =  AddtionTime / (Maximum - Minimal);

            if (value > Maximum)
                value = Maximum;
            if (value < Minimal)
                value = Minimal;
            double addtime = rate * (value - Minimal);
            return (int)Math.Round(addtime, 0);
        }

        protected TimeSetting()
        { }

        public TimeSetting(Queue queue)
        {
			Queue = queue;
        }

    }

}
