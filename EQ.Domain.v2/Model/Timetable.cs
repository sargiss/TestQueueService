using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Назначение очереди в окно
    /// </summary>
    public abstract class Timetable
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Окно
        /// </summary>
        public virtual long ResourceId { get; set; }

        /// <summary>
        /// Очередь
        /// </summary>
        public virtual Queue Queue { get; set; }

        /// <summary>
        /// Время начала приема с начала дня в минутах
        /// </summary>
        public virtual int TimeStart { get; set; }

        public virtual int TimeStartNow
        {
            get
            {
                int time_now = (int)(DateTime.Now - DateTime.Now.Date).TotalMinutes;
                return Math.Max(time_now, TimeStart);
            }
        }

        /// <summary>
        /// Время окончания приема с начала дня в минутах
        /// </summary>
        public virtual int TimeEnd { get; set; }


        /// <summary>
        /// Является ли это расписание на текущий день
        /// </summary>
        /// <returns></returns>
        public abstract bool IsToday();

        /// <summary>
        /// Является ли это расписание на указанную дату
        /// </summary>
        /// <returns></returns>
        public abstract bool IsDay(DateTime date);

        public virtual int TimeInterval
        {
            get 
            {
                int time_now = (int)(DateTime.Now - DateTime.Now.Date).TotalMinutes;
                if (TimeEnd > TimeStart && TimeEnd > time_now)
                {
                    return TimeEnd - TimeStartNow;
                }
                else
                    return 0;
            }
        }

	    public virtual TimeSpan TimeSpanStart
	    {
		    get { return TimeSpan.FromMinutes(TimeStart); }
			set { TimeStart = (int)value.TotalMinutes; }
	    }

	    public virtual TimeSpan TimeSpanEnd
	    {
		    get { return TimeSpan.FromMinutes(TimeEnd); }
			set { TimeEnd = (int)value.TotalMinutes; }
	    }

		/// <summary>
		/// Клонирование элемента расписания для другой очереди
		/// </summary>
		public virtual Timetable Clone(Queue queue)
		{
			Timetable clone = (Timetable)MemberwiseClone();
			clone.Id = 0;
			clone.Queue = queue;
			return clone;
		}
    }

    /// <summary>
    /// Общее расписание для окна
    /// </summary>
    public class CommonTimetable : Timetable
    {
        /// <summary>
        /// День недели 
        /// </summary>
        public virtual DayOfWeek Day { get; set; }

        /// <summary>
        /// Является ли это расписание на текущий день
        /// </summary>
        /// <returns></returns>
        public override bool IsToday()
        {
            return DateTime.Now.DayOfWeek == Day;
        }


        public override bool IsDay(DateTime date)
        {
            return date.DayOfWeek == Day;
        }
    }

    /// <summary>
    /// индивидуальное расписание для окна на определенную дату
    /// </summary>
    public class IndividualTimetable : Timetable
    {
        /// <summary>
        /// Индивидуальное расписание начиная с указанной даты
        /// </summary>
        public virtual DateTime DateStart { get; set; }

        /// <summary>
        /// Индивидуальное расписание заканчивая указанной датой
        /// </summary>
        public virtual DateTime DateEnd { get; set; }

        /// <summary>
        /// Является ли это расписание на текущий день
        /// </summary>
        /// <returns></returns>
        public override bool IsToday()
        {
            return DateTime.Now >= DateStart && DateTime.Now < DateEnd;
        }

        public override bool IsDay(DateTime date)
        {
            return date >= DateStart && date < DateEnd;
        }
    }

}
