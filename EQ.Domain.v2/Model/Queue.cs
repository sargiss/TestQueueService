using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
	public enum QueueType
	{
		Live = 10,
		Prerecord = 20,
	}

    /// <summary>
    /// Очередь
    /// </summary>
    public abstract class Queue: ICloneable
    {
        public virtual long Id { get; protected set; }

		public virtual QueueType Type { get; protected set; }

        /// <summary>
        /// Отдел
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// Наименование очереди
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Наименование очереди для отображения на киоске
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Описание очереди (информация выводится на информационном киоске)
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public virtual string Remark { get; set; }

        /// <summary>
        /// Продолжительность приема в минутах
        /// </summary>
        public virtual int Duration { get; set; }

        /// <summary>
        /// Родительская очередь
        /// </summary>
        public virtual Queue Parent { get; set; }

        /// <summary>
        /// Код родительской очереди
        /// </summary>
        public virtual long ParentId
        {
            get
            {
                if (Parent == null)
                    return 0;
                else
                    return Parent.Id;
            }
        }

        /// <summary>
        /// Дочерние очереди
        /// </summary>
		public virtual IList<Queue> Children { get; set; }

        /// <summary>
        /// Количество талонов для уникального посетителя
        /// </summary>
        public virtual int UniqueTicketCount { get; set; }

        /// <summary>
        /// Префикс очереди
        /// </summary>
        public virtual string Prefix { get; set; }

        /// <summary>
        /// Возможность записаться в очередь
        /// </summary>
        public virtual bool Enable { get; set; }

        /// <summary>
        /// Порядок очереди в списке
        /// </summary>
        public virtual int Order { get; set; }

        /// <summary>
        /// Категория очереди
        /// </summary>
        public virtual Group Group { get; set; }
      
        /// <summary>
        /// Список источников, с которых можно записаться в очередь
        /// </summary>
		public virtual IList<Source> Sources { get; set; }

        public virtual string ImpossibleRecordMessage { get; set; }

		public virtual IList<TimeSetting> TimeSettings { get; set; }

        /// <summary>
        /// Настройки строк
        /// </summary>
		public virtual IList<LineSetting> LineSettings { get; set; }

        /// <summary>
        /// Список параметров выбора
        /// </summary>
		public virtual IList<OptionSetting> OptionSettings { get; set; }

        /// <summary>
        /// Список всех возможных предопределённых значений для параметров выбора
        /// </summary>
        public virtual ReadOnlyCollection<OptionSettingValue> OptionSettingsValues
        {
            get
            {
                return new ReadOnlyCollection<OptionSettingValue>(OptionSettings.SelectMany(p => p.OptionSettingValues).ToList());
            }
        }

		protected virtual IList<QueueInWindow> QueueInWindowList { get; set; }

        public virtual bool RemoveTimetable(Timetable elem)
        {
            if (TimetableList.Contains(elem))
            {
				TimetableList.Remove(elem);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Расписание для очереди
        /// </summary>
		public virtual IList<Timetable> TimetableList { get; set; }

		/// <summary>
		/// Список общих табло, в которые назначеная данная очередь (инвертированная коллекция)
		/// </summary>
		public virtual IList<CommonTablo> CommonPanels { get; set; }

        public Queue()
        {
            Children = new List<Queue>();
            Sources = new List<Source>();
            TimeSettings = new List<TimeSetting>();
            LineSettings = new List<LineSetting>();
            OptionSettings = new List<OptionSetting>();
			TimetableList = new List<Timetable>();
            QueueInWindowList = new List<QueueInWindow>();
	        CommonPanels = new List<CommonTablo>();
        }

        protected Queue(Queue sourceQueue): this()
        {
            Type = sourceQueue.Type;
            Name = sourceQueue.Name;
            Duration = sourceQueue.Duration;
            Prefix = sourceQueue.Prefix;
            Enable = sourceQueue.Enable;
            UniqueTicketCount = sourceQueue.UniqueTicketCount;
            Description = sourceQueue.Description;
            Remark = sourceQueue.Remark;
            ImpossibleRecordMessage = sourceQueue.ImpossibleRecordMessage;
            Order = sourceQueue.Order;

            // Скопируем источники формирования талона
            if (sourceQueue.Sources != null)
                foreach (Source source in sourceQueue.Sources)
                    Sources.Add(source);

            // Скопируем настройки полей ввода количества объектов для записи в очередь
            if (sourceQueue.TimeSettings != null)
                foreach (TimeSetting timeSetting in sourceQueue.TimeSettings)
                    TimeSettings.Add(new TimeSetting(this)
                    {
                        Name = timeSetting.Name,
                        Minimal = timeSetting.Minimal,
                        Maximum = timeSetting.Maximum,
                        Normal = timeSetting.Normal,
                        AddtionTime = timeSetting.AddtionTime,
                    });

            // Скопируем настройки произвольных полей ввода для записи в очередь
            if (sourceQueue.LineSettings != null)
                foreach (LineSetting lineSetting in sourceQueue.LineSettings)
                    LineSettings.Add(new LineSetting(this)
                    {
                        Caption = lineSetting.Caption,
                        Require = lineSetting.Require,
                        LineCount = lineSetting.LineCount,
                        Regex = lineSetting.Regex,
                    });

            // Скопируем настройки полей выбора из списка для записи в очередь
            if (sourceQueue.OptionSettings != null)
                foreach (OptionSetting clone in sourceQueue.OptionSettings.Select(x => x.Clone(this)))
                    OptionSettings.Add(clone);

            // Скопируем расписание
            if (sourceQueue.TimetableList != null)
                foreach (Timetable clone in sourceQueue.TimetableList.Select(x => x.Clone(this)))
                    TimetableList.Add(clone);
        }

        /// <summary>
        /// Список окон, которые определены в эту очередь
        /// </summary>
        public virtual ReadOnlyCollection<Window> Windows
        {
            get
            {
                IList<Window> lst = new List<Window>();
                foreach (QueueInWindow qw in QueueInWindowList)
                    if (!lst.Contains(qw.Window))
                        lst.Add(qw.Window);
                return new ReadOnlyCollection<Window>(lst);
            }
        }

        /// <summary>
        /// Вычисляем среднее время приема стандартного талона для текущей очереди
        /// </summary>
        /// <returns></returns>
        public virtual int GetAvgTicketTime()
        {
            int add_time = 0;
            foreach (TimeSetting ts in TimeSettings)
                add_time = add_time + ts.CalculateTime(ts.Normal);
            return (int)Duration + add_time;
        }

        /// <summary>
        /// Полный путь к очереди в иерархии
        /// </summary>
        public virtual string FullPathName
        {
            get
            {
                string name = Name;
                return Util.BuildQueueFullPathName(this, ref name);
            }
        }

        /// <summary>
        /// Признак портальной очереди
        /// </summary>
        public virtual bool IsPortal
        {
            get { return false; }
        }

        public virtual bool AddSource(Source source)
        {
            if (Sources.Contains(source))
            {
                Sources.Add(source);
                return true;
            }
            return false;
        }

        public virtual bool RemoveLineSetting(LineSetting elem)
        {
            if (LineSettings.Contains(elem))
            {
                LineSettings.Remove(elem);
                return true;
            }

            return false;
        }

        public virtual bool RemoveTimeSetting(TimeSetting elem)
        {
            if (TimeSettings.Contains(elem))
            {
                TimeSettings.Remove(elem);
                return true;
            }

            return false;
        }

        public abstract Queue CloneQueue();

        /// <summary>
        /// Реализация ICloneable
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return CloneQueue(); 
        }
    }

    /// <summary>
    /// Живая очередь
    /// </summary>
    public class LiveQueue : Queue
    {
        public virtual bool EnableAdditionalTicket { get; set; }
        public virtual bool EnableUnguaranteedTicket { get; set; }

        /// <summary>
        /// Вопрос заявителю перед записью в ы
        /// </summary>
        public virtual string PreRecordQuestion { get; set; }

        /// <summary>
        /// Сообщение при отрицательном ответе на вопрос
        /// </summary>
        public virtual string PreRecordAnswer { get; set; }

        /// <summary>
        /// Вопрос заявителю о праве приема без очереди
        /// </summary>
        public virtual string PreRecordPreferentialQuestion { get; set; }
        
        /// <summary>
        /// Количество дополнительных талонов на текущий день
        /// </summary>
        public virtual int AdditionalTicketCount { get; protected set; }

        /*
        /// <summary>
        /// Вычисляем количество оставшегося времени в минутах с учетом дополнительных талонов
        /// </summary>
        /// <returns></returns>
        public virtual int GetFreeTime()
        {
            int value = 0;
            foreach (Window w in GetWindows())
            {
                int const_add_window_time = 0; //возможно добавить параметр, который позволял бы учитывать отклонение (скажем если осталось 25 минут, в время среднего талона 30, то соответственно доступных талонов нет, но если добавить еще 15 минут, то норм
                int cur_val = w.GetFreeTime(this);
                if (cur_val + const_add_window_time >= GetAvgTicketTime())
                    value = value + cur_val;
            }

            if (AdditionalTicketCount != 0)
                return value + AdditionalTicketCount * GetAvgTicketTime();
            return value;
        }
        */

        public override Queue CloneQueue()
        {
            return new LiveQueue(this);
        }

		public LiveQueue()
		{
			Type = QueueType.Live;
		}

        protected LiveQueue(LiveQueue sourceQueue): base(sourceQueue)
        {
            EnableAdditionalTicket = sourceQueue.EnableAdditionalTicket;
            EnableUnguaranteedTicket = sourceQueue.EnableUnguaranteedTicket;
            PreRecordAnswer = sourceQueue.PreRecordAnswer;
            PreRecordQuestion = sourceQueue.PreRecordQuestion;
            PreRecordPreferentialQuestion = sourceQueue.PreRecordPreferentialQuestion;
        }
    }

    /// <summary>
    /// Предварительная очередь
    /// </summary>
    public class PrerecordQueue : Queue
    {
        /// <summary>
        /// Идентификатор очереди на портале Росреестра
        /// </summary>
        public virtual string PortalId { get; set; }

        /// <summary>
        /// Начало периода для записи 0 - с текущего дня
        /// </summary>
        public virtual int StartRecordInverval { get; set; }

        /// <summary>
        /// Завершение периода для записи. EndRecordInverval > StartRecordInverval
        /// StartRecordInverval = 1
        /// EndRecordInverval = 3 - означает, что записаться можно с завтрашнего дня и на 2 следующих
        /// </summary>
        public virtual int EndRecordInverval { get; set; }

        /// <summary>
        /// Возможность выбора даты
        /// </summary>
        public virtual bool EnableChooseDate { get; set; }

        /// <summary>
        /// Возможность выбора времени
        /// </summary>
        public virtual bool EnableChooseTime { get; set; }

        /// <summary>
        /// Признак портальной очереди
        /// </summary>
        public override bool IsPortal
        {
            get
            {
                return !string.IsNullOrEmpty(PortalId);
            }
        }

        public override Queue CloneQueue()
        {
            return new PrerecordQueue(this);
        }

		public PrerecordQueue()
		{
			Type = QueueType.Prerecord;
		}

        protected PrerecordQueue(PrerecordQueue sourceQueue):base(sourceQueue) 
        {
            StartRecordInverval = sourceQueue.StartRecordInverval;
            EndRecordInverval = sourceQueue.EndRecordInverval;
            EnableChooseDate = sourceQueue.EnableChooseDate;
            EnableChooseTime = sourceQueue.EnableChooseTime;
        }
    }
}
