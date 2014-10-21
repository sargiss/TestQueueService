using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Дополнительный талон
    /// </summary>
    public class AdditionalTicket
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Дата формирования дополнительного талона
        /// </summary>
        public virtual DateTime Date { get; protected set; }

        /// <summary>
        /// Дополнительный талон для очереди
        /// </summary>
        public virtual Queue Queue { get; set; }

        /// <summary>
        /// Доп. талон для указанного окна
        /// </summary>
        public virtual Window Window { get; set; }

        /// <summary>
        /// Кол-во доп. талонов. "+" - добавление талонов, "-" - аннулирование талонов
        /// </summary>
        public virtual int Count { get; set; }

        /// <summary>
        /// Доп. талоны для указанного пользователя
        /// </summary>
        public virtual User User { get; set; }

        public AdditionalTicket()
        {
            Date = DateTime.Now;
        }
    }

}
