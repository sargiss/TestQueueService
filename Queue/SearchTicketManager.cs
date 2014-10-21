using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using log4net;
using NHibernate;
using NHibernate.Criterion;

using EQ.Core.Common;
using EQ.DAL.v1;
using EQ.Domain.v2;
using NHibernate.SqlCommand;


namespace EQ.Core.Operator
{
    /// <summary>
    /// Менеджер поиска талонов в очередях
    /// </summary>
    internal class SearchTicketManager : IDisposable
    {
        private int _callTimeBefore;
        private int _maxCallAttemptBefore;
        private int _maxCallAttemptAfter;
        private int _repeatCallInterval;
        private TimeSpan _repeatCallTimeout;
        private readonly object padlock = new object();

        /// <summary>
        /// Менеджер поиска талонов в очередях
        /// </summary>
        /// <param name="callTimeBefore">Вызывать за указанное количество минут до назначенного времени (только для предварительных талонов)</param>
        /// <param name="maxCallAttemptBefore">Количество вызовов до назначенного времени (только для предварительных талонов)</param>
        /// <param name="maxCallAttemptAfter">Количество вызовов после назначенного времени (только для предварительных талонов)</param>
        /// <param name="repeatCallInterval">Пауза между вызывами талона (только для предварительных талонов) (в минутах)</param>
        /// <param name="repeatCallTimeout">Время ожидания талона, если очередь пустая (в секундах)</param>
        public SearchTicketManager(int callTimeBefore, int maxCallAttemptBefore, int maxCallAttemptAfter, int repeatCallInterval, int repeatCallTimeout)
        {
            _callTimeBefore = callTimeBefore;
            _maxCallAttemptBefore = maxCallAttemptBefore;
            _maxCallAttemptAfter = maxCallAttemptAfter;
            _repeatCallInterval = repeatCallInterval;
            _repeatCallTimeout = new TimeSpan(0, 0, repeatCallTimeout);
            OperatorSessionManager.SessionFree += new SessionEventHandler(GetNextTicket);
        }

        private void GetNextTicket(string sessionKey)
        {
            OperatorSession session = OperatorSessionManager.Instance.Get(sessionKey);
            if (session != null && session.TicketId <= 0)
            {
                try
                {
                    using (ISession s = SessionHelper.OpenSession())
                    {
                        if (session.Status != Status.Free)
                            return;
                        Ticket ticket = null;
                        lock (padlock)
                        {
                            ticket = searchTicketForWindow(s, session.Window.Id);
                        }
                        if (ticket != null)
                        {
                            if (session.Status != Status.Free)
                                return;
                            LockTicketManager.Instance.Add(ticket.Id); // Оченя важный момент!!!
                            Log.GetLog(typeof(SearchTicketManager)).InfoFormat("Вызов клиента c талоном {0}, ticketId={1} windowId={2} sessionKey={3}", ticket.Number, ticket.Id, session.Window.Id, sessionKey);
                            EQ.External.Domain.Ticket ext_ticket = EQ.Core.Common.Transform.transform(ticket);
                            OperatorSessionManager.Instance.CallClient(sessionKey, ext_ticket);
                        }
                        else
                        {
                            if (session.Status != Status.Free)
                                return;
                            Log.GetLog(typeof(SearchTicketManager)).InfoFormat("Нет талонов в очереди для окна {0}, windowId={1} sessionKey={2}", session.Window.Name, session.Window.Id, session.SessionKey);
                            new Thread(WaitTicket).Start(sessionKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.GetLog(typeof(SearchTicketManager)).Error(ex.Message, ex);
                    if (session.Status != Status.Free)
                        return;
                    new Thread(WaitTicket).Start(sessionKey);
                }
            }
        }

        private void WaitTicket(object o)
        {
            string sessionKey = (string)o;
            OperatorSession session = OperatorSessionManager.Instance.Get(sessionKey);
            if (session != null)
            {
                Log.GetLog(typeof(SearchTicketManager)).InfoFormat("Ожидание клиента, окно {0}, windowId={1} sessionKey={2}", session.Window.Name, session.Window.Id, session.SessionKey);
                Thread.Sleep(_repeatCallTimeout);
                GetNextTicket(sessionKey);
            }
        }

        public void Dispose()
        {
            OperatorSessionManager.SessionFree -= new SessionEventHandler(GetNextTicket);
        }


        Dictionary<long, IList<QueueInWindow>> queueOnWindows = new Dictionary<long, IList<QueueInWindow>>();
        /// <summary>
        /// Поиск талона для окна
        /// </summary>
        private Ticket searchTicketForWindow(ISession session, long windowId)
        {
            if (!queueOnWindows.ContainsKey(windowId))
            {
                IList<QueueInWindow> queue_window_lst_pre = session.CreateCriteria<QueueInWindow>()
                                                .Add(Restrictions.Eq("Window.Id", windowId))
                                                .AddOrder(Order.Desc("Priority"))
                                                .List<QueueInWindow>();

                //создаем список очередей, для которых на текуший момент времени определено раписание и которые работают
                IList<QueueInWindow> queue_window_lst1 = new List<QueueInWindow>();
                if (queue_window_lst_pre != null && queue_window_lst_pre.Count > 0)
                    foreach (QueueInWindow qw in queue_window_lst_pre)
                        if (qw.Queue != null)
                        {
                            EQ.External.Domain.Map map = EQ.Core.Signup.QueueHelper.GetCrossMap(session, qw.Queue, DateTime.Now);
                            if (map != null && map.IsFreeBefore(DateTime.Now))
                                queue_window_lst1.Add(qw);
                        }
                queueOnWindows[windowId] = queue_window_lst1;
            }

            var queue_window_lst = queueOnWindows[windowId];

            //Если нет никаких очередей в окне
            if (queue_window_lst.Count == 0)
            {
                return null;
            }
            else if (queue_window_lst.Count == 1) //Если в окно определена только 1 очередь
            {
                return getNextTicketFromQueue(session, queue_window_lst[0].Queue, windowId);
            }
            else //если в окно определена более, чем 1 очередь
            {
                //создаем словарь (приоритет, список очередей)
                IDictionary<int, List<Queue>> queue_dic = queue_window_lst.GroupBy(x => x.Priority, x => x.Queue).ToDictionary(s => s.Key, s => s.ToList());
                foreach (KeyValuePair<int, List<Queue>> elem in queue_dic)
                {
                    Ticket ticket = null;

                    if (elem.Value.Count == 1) //Если нет несколько очередей с одинаковым приоритетом
                    {
                        ticket = getNextTicketFromQueue(session, elem.Value[0], windowId);
                    }
                    else //Если 2 и более очередей имееют одинаковый приориет
                    {
                        List<Queue> pre_queue_lst = new List<Queue>();
                        List<Queue> live_queue_lst = new List<Queue>();

                        foreach (Queue q in elem.Value) //Распределяем очереди по типу
                            if (typeof(LiveQueue).IsAssignableFrom(NHibernateUtil.GetClass(q)))
                                live_queue_lst.Add(q);
                            else if (typeof(PrerecordQueue).IsAssignableFrom(NHibernateUtil.GetClass(q)))
                                pre_queue_lst.Add(q);

                        if (pre_queue_lst.Count > 0 && live_queue_lst.Count > 0)
                        {
                            Log.GetLog(typeof(SearchTicketManager)).WarnFormat("В окно не могут быть назначены разные типы очередей с одинаковым приоритетом. Проверьте настройки системы. windowId={0}",windowId);
                            return null;
                        }

                        //Обрабатываем живые очереди
                        if (live_queue_lst.Count > 0)
                            ticket = getNextTicketFromQueue(session, live_queue_lst.ToArray(), windowId);

                        //Обрабатываем предварительные очереди
                        if (pre_queue_lst.Count > 0)
                            ticket = getNextTicketFromQueue(session, pre_queue_lst.ToArray(), windowId);
                    }

                    if (ticket != null)
                        return ticket;
                }
            }
            return null;
        }

        /// <summary>
        /// поиск талона в очереди
        /// </summary>
        /// <param name="session"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        private Ticket getNextTicketFromQueue(ISession session, Queue queue, long windowId)
        {
            return getNextTicketFromQueue(session,new Queue[]{queue}, windowId);
        }
        
        /// <summary>
        ///  поиск талона в списке очередей одного типа
        /// </summary>
        private Ticket getNextTicketFromQueue(ISession session, Queue[] queueList, long windowId)
        {
            if (queueList == null || queueList.Length == 0)
                return null;
            
            //определяем по первому элементу тип очереди (на входе будут только очереди одного типа)
            if (typeof(LiveQueue).IsAssignableFrom(NHibernateUtil.GetClass(queueList[0])))
            {
                ICriteria cr1 = session.CreateCriteria<LiveTicket>()
											.CreateAlias("PreviousTicket", "prevTicket", JoinType.LeftOuterJoin)
											.Add(Restrictions.Between("Date", DateTime.Now.Date, DateTime.Now.Date.AddDays(1).AddSeconds(-1)))
                                            .Add(Restrictions.In("Queue", queueList))
                                            .Add(Restrictions.Eq("CurrentStatus", TicketStatus.Open))
                                            .Add(Restrictions.Or(Restrictions.IsNull("Window"), Restrictions.Eq("Window.Id",windowId)))
											.Add(Restrictions.Or(Restrictions.IsNull("PreviousTicket"), Restrictions.Eq("prevTicket.CurrentStatus", TicketStatus.Close)))
                                            .AddOrder(Order.Desc("Priority"))
                                            .AddOrder(Order.Asc("Date"))
                                            .SetMaxResults(1);
                
                IList<long> ticketInProc1 =  OperatorSessionManager.Instance.GetTicketInProcess();
                if (ticketInProc1 != null && ticketInProc1.Count > 0)
                    cr1.Add(!Restrictions.In("Id", ticketInProc1.ToArray()));

                return cr1.UniqueResult<LiveTicket>();
            }
            else if (typeof(PrerecordQueue).IsAssignableFrom(NHibernateUtil.GetClass(queueList[0])))
            {
                int now = (int)(DateTime.Now - DateTime.Now.Date).TotalMinutes;
                ICriteria cr2 = session.CreateCriteria<PrerecordTicket>()
                                                .Add(Restrictions.Between("RecordDate", DateTime.Now.Date, DateTime.Now.Date.AddDays(1).AddSeconds(-1)))
                                                .Add(Restrictions.In("Queue", queueList))
                                                .Add(Restrictions.Eq("CurrentStatus", TicketStatus.Open))
                                                .Add(Restrictions.Le("RecordStart", now + _callTimeBefore))
                                                .Add(Restrictions.Lt("CallNumberBefore", _maxCallAttemptBefore))
                                                .Add(Restrictions.Eq("CallNumberAfter", 0))
                                                .Add(Restrictions.Lt("LastCallTime", now - _repeatCallInterval))
                                                .Add(Restrictions.Or(Restrictions.IsNull("Window"), Restrictions.Eq("Window.Id", windowId)))
                                                .AddOrder(Order.Asc("RecordStart"))
                                                .SetMaxResults(1);
                IList<long> ticketInProc2 = OperatorSessionManager.Instance.GetTicketInProcess();
                if (ticketInProc2 != null && ticketInProc2.Count > 0)
                    cr2.Add(!Restrictions.In("Id", ticketInProc2.ToArray()));
                PrerecordTicket pt1 = cr2.UniqueResult<PrerecordTicket>();
                
                if (pt1 != null)
                    return pt1;

                ICriteria cr3 = session.CreateCriteria<PrerecordTicket>()
                                                .Add(Restrictions.Between("RecordDate", DateTime.Now.Date, DateTime.Now.Date.AddDays(1).AddSeconds(-1)))
                                                .Add(Restrictions.In("Queue", queueList))
                                                .Add(Restrictions.Eq("CurrentStatus", TicketStatus.Open))
                                                .Add(Restrictions.Le("RecordStart", now))
                                                .Add(Restrictions.Lt("LastCallTime", now - _repeatCallInterval))
                                                .Add(Restrictions.Or(Restrictions.IsNull("Window"), Restrictions.Eq("Window.Id", windowId)))
                                                .AddOrder(Order.Asc("CallNumberAfter"))
                                                .AddOrder(Order.Desc("RecordStart"))
                                                .SetMaxResults(1);
                IList<long> ticketInProc3 = OperatorSessionManager.Instance.GetTicketInProcess();
                if (ticketInProc3 != null && ticketInProc3.Count > 0)
                    cr3.Add(!Restrictions.In("Id", ticketInProc3.ToArray()));
                return cr3.UniqueResult<PrerecordTicket>();
            }
            
            return null;
        }
    }
}
