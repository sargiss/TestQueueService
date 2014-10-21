using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Окно приема
    /// </summary>
    public class Window
    {
		protected virtual IList<QueueInWindow> QueueInWindowList { get; set; }

        public virtual long Id { get; protected set; }

        /// <summary>
        /// Номер окна
        /// </summary>
        public virtual int Number { get; set; }

        /// <summary>
        /// Наименование окна
        /// </summary>
        public virtual string Name { get; set; }

        public virtual Department Department { get; protected set; }

        private void Init()
        {
			QueueInWindowList = new List<QueueInWindow>();
        }

        protected Window()
        {
            Init();
        }

        public Window(Department department)
        {
            Department = department;
            Init();
        }

        /// <summary>
        /// Список очередей, в которые назначено это окно
        /// </summary>
        public virtual ReadOnlyCollection<Queue> QueueList
        {
            get
            {
                IList<Queue> lst = new List<Queue>();
				foreach (QueueInWindow qw in QueueInWindowList)
                    if (!lst.Contains(qw.Queue))
                        lst.Add(qw.Queue);
                return new ReadOnlyCollection<Queue>(lst);
            }
        }

    }

    /// <summary>
    /// Служебный класс для склеивания временных периодов (timetable)
    /// </summary>
    public class Period
    {
        public virtual int TimeStart { get; set; }
        public virtual int TimeEnd { get; set; }
        public virtual long ResourceId { get; protected set; }
        public virtual int TimeInterval
        {
            get
            {
                return TimeEnd > TimeStart ? TimeEnd - TimeStart : 0;  
            }
        }

        public static IList<Period> Glue(IList<Timetable> timetables)
        {
            return Glue(timetables, false);
        }
        /// <summary>
        /// Склеить периоды
        /// </summary>
        /// <param name="timetables">Список расписаний</param>
        /// <param name="isNow">На текущий момент. Для живой очереди = true, Для предварительной = false</param>
        /// <returns></returns>
        public static IList<Period> Glue(IList<Timetable> timetables, bool isNow)
        {
            IList<Period> pL = new List<Period>();
            IList<Period> pLT = new List<Period>();
            if (timetables == null)
                return pLT;

            foreach (Timetable t in timetables)
                pL.Add(new Period { TimeStart = isNow ? t.TimeStartNow : t.TimeStart, TimeEnd = t.TimeEnd, ResourceId=t.ResourceId });

            foreach (Period p in pL)
            {
                if (pLT.Count == 0)
                    pLT.Add(p);
                else
                {
                    bool insert_new = false;
                    foreach (Period pt in pLT)
                    {
                        if (p.TimeStart >= pt.TimeStart && p.TimeStart <= pt.TimeEnd && p.ResourceId == pt.ResourceId)
                        {
                            if (p.TimeEnd > pt.TimeEnd)
                                pt.TimeEnd = p.TimeEnd;
                        }
                        else if (p.TimeStart <= pt.TimeStart && p.TimeEnd >= pt.TimeStart && p.ResourceId ==pt.ResourceId)
                        {
                            pt.TimeStart = p.TimeStart;
                            if (p.TimeEnd > pt.TimeEnd)
                                pt.TimeEnd = p.TimeEnd;
                        }
                        else
                            insert_new = true;
                    }
                    if (insert_new)
                        pLT.Add(p);
                }
            }

            return pLT;
        }

        /// <summary>
        /// Включается ли заданный интервал в указанный
        /// </summary>
        /// <param name="period">интервал</param>
        /// <param name="timeStart">начало периода</param>
        /// <param name="timeEnd">окончание периода</param>
        public static bool IncludeInInterval(Period period, int timeStart, int timeEnd)
        {
            if (timeStart == 0 || timeEnd == 0)
                return false;
            return period.TimeStart <= timeStart && period.TimeEnd >= timeEnd;
        }

        /// <summary>
        /// Получить кол-во вхождений в интервал
        /// </summary>
        /// <param name="period">интервал</param>
        /// <param name="interval">период</param>
        /// <returns></returns>
        public static int GetNumberOfOccurrencesInInterval(Period period, int interval)
        {
            if (period.TimeInterval > interval && interval != 0)
                return (int)period.TimeInterval / interval;
            return 0;
        }
    }

}
