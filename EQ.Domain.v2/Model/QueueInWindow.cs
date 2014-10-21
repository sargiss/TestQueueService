using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Очередь в окне
    /// </summary>
    public class QueueInWindow
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Приорирет
        /// </summary>
        public virtual int Priority { get; set; }

        /// <summary>
        /// Очередь
        /// </summary>
        public virtual Queue Queue { get; set; }

        /// <summary>
        /// Окно
        /// </summary>
        public virtual Window Window { get;  set; }

        /// <summary>
        /// Название очереди
        /// </summary>
        public virtual string QueueName
        {
            get
            {
                if (Queue != null)
                    return Queue.Name;
                else
                    return string.Empty;
            }
        }

    }


}
