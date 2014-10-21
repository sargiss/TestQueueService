using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Информационный киоск
    /// </summary>
    public class Terminal
    {
        public virtual long Id { get; protected set; }

        public virtual string Key { get; set; }

        /// <summary>
        /// Наименование информационного киоска
        /// </summary>
        public virtual string Name { get; set; }
        
        /// <summary>
        /// Примечание
        /// </summary>
        public virtual string Remark { get; set; }

        /// <summary>
        /// Отдел
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// Отображать ли на киоске возможность записи
        /// </summary>
        public virtual bool EnableSignup { get; set; }

        /// <summary>
        /// Отображать ли на киоске справочную информацию
        /// </summary>
        public virtual bool EnableInfo { get; set; }

        /// <summary>
        /// Отображать ли на киоске анкету
        /// </summary>
        public virtual bool EnableQuestionnaire { get; set; }

        /// <summary>
        /// Отображать ли на киоске готовность документов
        /// </summary>
        public virtual bool EnableAvailabilityDocument { get; set; }

        /// <summary>
        /// Отображать ли на киоске ссылку на мобильный портал
        /// </summary>
        public virtual bool EnablePortal { get; set; }

	    public virtual IList<AssignedQueue> AssignedQueueList { get; set; }

		/// <summary>
		/// Разрешить запись одного заявителя на несколько услуг
		/// </summary>
		public virtual bool EnableMultiSignup { get; set; }

        public Terminal()
        {
			AssignedQueueList = new List<AssignedQueue>();
        }

        public virtual IList<Queue> GetQueues()
        {
			if (AssignedQueueList == null || AssignedQueueList.Count == 0)
                return null;

            IList<Queue> queue_lst = new List<Queue>();
			foreach (AssignedQueue aq in AssignedQueueList)
                if (!queue_lst.Contains(aq.Queue))
                    queue_lst.Add(aq.Queue);

            return queue_lst;
        }

        public virtual int GetQueueTimeout(Queue queue)
        {
            if (queue == null)
                return 0;
			foreach (AssignedQueue aq in AssignedQueueList)
                if (aq.Queue.Id == queue.Id)
                    return aq.QueueTimeout;
            return 0;

        }
    }


    public class AssignedQueue
    {
        public virtual long Id { get; protected set; }
        
        /// <summary>
        /// Информационный киоск
        /// </summary>
        public virtual Terminal Terminal { get; protected set; }
        
        /// <summary>
        /// Назначенная очередь
        /// </summary>
        public virtual Queue Queue { get; set; }
        
        /// <summary>
        /// За сколько до начала приема (из расписания очереди) отображать очередь (в минутах)
        /// </summary>
        public virtual int QueueTimeout { get; set; }

        /// <summary>
        /// Название очереди
        /// </summary>
        public virtual string QueueName
        {
            get { return Queue != null ? Queue.Name : string.Empty; }
        }

        /// <summary>
        /// Полный путь к очереди
        /// </summary>
        public virtual string QueueFullPathName
        {
            get { return Queue != null ? Queue.FullPathName : string.Empty; }
        }

        public AssignedQueue(Terminal terminal, Queue queue)
        {
            Terminal = terminal;
            Queue = queue;
        }

        protected AssignedQueue() { }
    }
}
