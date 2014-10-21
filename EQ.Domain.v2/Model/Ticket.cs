using EQ.Core.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
	public enum TicketType
	{
		Live = 10,
		Prerecord = 20,
	}

    /// <summary>
    /// Талон
    /// </summary>
    public abstract class Ticket
    {
        public virtual long Id { get; protected set; }

		public virtual TicketType Type { get; protected set; }

        /// <summary>
        /// Дата и время формирования талона
        /// </summary>
        public virtual DateTime Date { get; protected set; }

        /// <summary>
        /// Номер талона
        /// </summary>
        public virtual string Number { get; set; }

        /// <summary>
        /// Очередь
        /// </summary>
        public virtual Queue Queue { get; protected set; }

        /// <summary>
        /// Записать в указанное окно
        /// </summary>
        public virtual Window Window { get; set; }

        public virtual long ResourceId { get; set; }

        /// <summary>
        /// Оценка качества обслуживания талона
        /// </summary>
        public virtual ServiceQuality Quality { get; set; }

        /// <summary>
        /// Отдел
        /// </summary>
        public virtual Department Department
        {
            get
            {
                return Queue != null ? Queue.Department : null;
            }
        }

        /// <summary>
        /// Место формирования талона
        /// </summary>
        public virtual Source Source { get; set; }

		/// <summary>
		/// Идентификатор предыдущего талона (при множественной записи)
		/// </summary>
		public virtual Ticket PreviousTicket { get; set; }

	    /// <summary>
	    /// История обработки
	    /// </summary>
		public virtual IList<Process> ProcessList { get; set; }

        /// <summary>
        /// Текущий статус
        /// </summary>
        public virtual TicketStatus CurrentStatus { get; protected set; }

        /// <summary>
        /// Строки в талоне
        /// </summary>
		public virtual IList<TicketLineItem> Lines { get; protected set; }

        /// <summary>
        /// Добавить строчку в талон
        /// </summary>
        /// <param name="lineSetting"></param>
        /// <param name="value"></param>
        public virtual void AddLine(LineSetting lineSetting, string value)
        {
            if (lineSetting == null)
                throw new ArgumentNullException("lineSetting");
            Lines.Add(new TicketLineItem(this, lineSetting, value));
        }

        /// <summary>
        /// Дополнительное время
        /// </summary>
        public virtual IList<TicketTimeItem> AddtionalTimeParams { get; set; }

        public virtual void AddTimeItem(TimeSetting timeSetting, int value)
        {
            if (timeSetting == null)
                throw new ArgumentNullException("timeSetting");
			AddtionalTimeParams.Add(new TicketTimeItem(this, timeSetting, value));
        }

        /// <summary>
        /// Список дополнительных опций
        /// </summary>
		public virtual IList<TicketOptionItem> Options { get; set; }

        /// <summary>
        /// Добавляет опцию в талон
        /// </summary>
        public virtual void AddOptionItem(OptionSettingValue optionSettingValue)
        {
            if (optionSettingValue == null)
                throw new ArgumentNullException("optionSettingValue");

            Options.Add(new TicketOptionItem(this, optionSettingValue));
        }
        
        private void Init()
        {
			ProcessList = new List<Process>();
            Lines = new List<TicketLineItem>();
            AddtionalTimeParams = new List<TicketTimeItem>();
            Options = new List<TicketOptionItem>();
            Date = DateTime.Now;
        }
        
        protected Ticket()
        {
            Init();
        }

        public Ticket(Queue queue)
        {
            Init();
            Queue = queue;
        }

        /// <summary>
        /// Дополнительное время талона (рассчитывается на основе временых параметров)
        /// </summary>
        /// <returns></returns>
        public virtual int GetAdditionalTime()
        {
            int value = 0;

            foreach (TicketTimeItem t in AddtionalTimeParams)
                value = value + t.AddtionTime;

            foreach (TicketOptionItem option in Options)
                value = value + option.OptionSettingValue.AdditionalTime;

            return value;
        }

        /// <summary>
        /// Расчетное время обслуживания талона
        /// </summary>
        /// <returns></returns>
        public virtual int GetTotalTime()
        {
            int value = GetAdditionalTime();
            return value + Queue.Duration;
        }

        public virtual void SetStatus(TicketStatus status)
        {
            SetStatus(status, string.Empty, null);
        }

        public virtual void SetStatus(TicketStatus status, string remark)
        {
            SetStatus(status, remark, null);
        }

        public virtual void SetStatus(TicketStatus status, string remark, Window window)
        {
            SetStatus(status, remark, window, null);
        }

        public virtual void SetStatus(TicketStatus status, string remark, Window window, User user)
        {
            if (CurrentStatus == status)
                return;
			ProcessList.Add(new Process { Ticket = this, Status = status, Remark = remark, Window = window, User = user });
        }
        public virtual void SetStatus(Process process)
        {
            if (process == null || process.Status == CurrentStatus)
                return;
            if (process.Ticket != null && process.Ticket.Id == Id)
				ProcessList.Add(process);
        }

        /// <summary>
        /// Время (в минутах с начала дня) последнего вызова
        /// </summary>
        public virtual int LastCallTime {get; protected set;}
    }

    /// <summary>
    /// Талон для живой очереди
    /// </summary>
    public class LiveTicket : Ticket
    {
        /// <summary>
        /// Является ли гарантированным талон
        /// </summary>
        public virtual bool IsGuaranteed { get; set; }

        /// <summary>
        /// Приоритет
        /// </summary>
        public virtual int Priority { get; set; }

        /// <summary>
        /// Необходимое время для обслуживания талона
        /// </summary>
        public virtual int Duration { get; set; }

        protected LiveTicket(): base()
        { }

        public LiveTicket(Queue queue) : base(queue)
        { }

        public LiveTicket(Queue queue, Ticket source)
            : base(queue)
        {
            if (source == null)
                throw new ArgumentException("Параметр ticket должен быть определен");

            Source = source.Source;
            ResourceId = source.ResourceId;
            Number = source.Number;
            Duration = source.GetTotalTime();

            if (source.Lines != null)
                foreach (var l in source.Lines)
                    Lines.Add(l.Clone(this));

            Parent = source;
	        PreviousTicket = source.PreviousTicket;
        }

        public virtual Ticket Parent { get; set; }

        public override int GetTotalTime()
        {
            if (Duration > 0)
                return Duration;
            else
                return base.GetTotalTime();
        }
    }

    /// <summary>
    /// Талон для предварительной очереди
    /// </summary>
    public class PrerecordTicket : Ticket
    {
        /// <summary>
        /// Портальный идентификатор талона
        /// </summary>
        public virtual string PortalId { get; set; }

        /// <summary>
        /// Дата  записи (только для предварительной записи)
        /// </summary>
        public virtual DateTime RecordDate { get; set; }
        
        /// <summary>
        /// Время записи с (в минутах с начала дня)
        /// </summary>
        public virtual int RecordStart { get; set; }

        /// <summary>
        /// Время записи до (в минутах с начала дня)
        /// </summary>
        public virtual int RecordEnd { get; set; }
        
        protected PrerecordTicket(): base()
        { }

        public PrerecordTicket(Queue queue): base(queue)
        { }

        public PrerecordTicket(Queue queue, DateTime date) : base(queue)
        {
            Date = date;
        }

        /// <summary>
        /// Количество вызовов до начала запланированного времени приема
        /// </summary>
        public virtual int CallNumberBefore {get; protected set;}

        /// <summary>
        /// Количество вызовов после при запланированного времени приема
        /// </summary>
		public virtual int CallNumberAfter { get; protected set; }

	    public virtual DateTime RecordDateTime
	    {
			get { return RecordDate.Date.AddMinutes(RecordStart); }
	    }

        public override int GetTotalTime()
        {
            return RecordEnd - RecordStart;
        }
    }

}
