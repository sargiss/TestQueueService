using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EQ.Domain.v2
{
    [DataContract]
    public enum LineItemState
    {
        [EnumMember]
        NoRecord, //Нет возможности записаться
        [EnumMember]
        Busy, //Занято
        [EnumMember]
        Free //Свободно
    }

    /// <summary>
    /// Карта занятости(используется для рассчета времени записи в предварительную очередь)
    /// </summary>
    [DataContract]
    public class LineBusyMap
    {
        private static int _max_value = 1440;

        /// <summary>
        /// false - занятое время, true - свободно
        /// </summary>
        [DataMember]
        public LineItemState[] Line { get; protected set; }

        /// <summary>
        /// Идентификатор окна
        /// </summary>
        [DataMember]
        public long ResourceId { get; protected set; }

	    private LineBusyMap()
	    {
	    }

		public LineBusyMap(long resourceId)
        {
            ResourceId = resourceId;
            Line = new LineItemState[_max_value];
        }

	    public LineBusyMap Clone()
	    {
		    LineBusyMap result = (LineBusyMap)MemberwiseClone();
		    result.Line = (LineItemState[])result.Line.Clone();
		    return result;
	    }


        /// <summary>
        /// Заполнить интервал
        /// </summary>
        /// <param name="timeStart">Время начала интервала</param>
        /// <param name="interval">Продолжительность=</param>
        /// <param name="flag">true - освободить интервал, false - заблокировать интервал</param>
        private void FillInterval(int timeStart, int interval, LineItemState state)
        {
            if (interval > 0)
                for (int i = timeStart; i < (timeStart + interval); i++)
                    if (i < _max_value)
                        Line[i] = state;
        }

        public void FillLine(int timeStart, int timeEnd, LineItemState state)
        {
            if (timeEnd > timeStart)
                FillInterval(timeStart, timeEnd - timeStart,state);
        }

        private void BusyInterval(int timeStart, int interval)
        {
            if (interval > 0)
                for (int i = timeStart; i < (timeStart + interval); i++)
                    if (i < _max_value && Line[i] == LineItemState.Free)
                        Line[i] = LineItemState.Busy;
        }

        public void BusyLine(int timeStart, int timeEnd)
        {
            if (timeEnd > timeStart)
                BusyInterval(timeStart, timeEnd - timeStart);
        }

        /// <summary>
        /// Найти минимальное время для записи
        /// </summary>
        /// <param name="interval">Требуемое время</param>
        /// <returns>Время начала(</returns>
        public int GetMinStartTime(int interval)
        {
            int counter = 0;
            for (int i = 0; i < _max_value; i++)
            {
                if (Line[i] == LineItemState.Free)
                    counter++;
                else
                    counter = 0;

                if (counter == interval)
                    return i - counter + 1;
            }
            return _max_value;
        }

        /// <summary>
        /// Можно ли записаться в заданный интервал
        /// </summary>
        /// <param name="timeStart">Время начала записи</param>
        /// <param name="timeEnd">Время окончания записи</param>
        /// <returns>Результат</returns>
        public bool IsOpenInterval(int timeStart, int timeEnd)
        {
            if ((timeEnd - timeStart) <= 0)
                return false;

            for (int i = timeStart; i < timeEnd; i++)
                if (i < _max_value && Line[i] != LineItemState.Free)
                    return false;

            return true;
        }

        /// <summary>
        /// получить состояние в карте на заданное время
        /// </summary>
        public LineItemState GetStateByTime(DateTime datetime)
        {
            int cur_time = (int)(datetime - datetime.Date).TotalMinutes;
            return Line[cur_time];
        }
        
        /// <summary>
        /// Было ли определено расписание на дату до указанной
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public bool IsFreeBefore(DateTime datetime)
        {
            int cur_time = (int)(datetime - datetime.Date).TotalMinutes;
            for (int i = 0; i < cur_time; i++)
                if (Line[i] != LineItemState.NoRecord)
                    return true;
            return false;
        }

        public bool IsAfterNoRecord(DateTime datetime)
        {
            int cur_time = (int)(datetime - datetime.Date).TotalMinutes;
            for (int i = cur_time; i < _max_value; i++)
                if (Line[i] != LineItemState.NoRecord)
                    return false;
            return true;

        }

        /// <summary>
        /// Зарегистрировать ближайшее время для талона
        /// </summary>
        /// <param name="maps">Список карт</param>
        /// <param name="interval">Требуемое время</param>
        /// <param name="timeStart">Время начала записи</param>
        /// <param name="isLastAvailableTicket">Признак того, что для записи остался доступен последний талон</param>
        /// <returns>true - записан, false - отказ</returns>
		public static bool RegisterInterval(IList<LineBusyMap> maps, int interval, out int timeStart, out long windowId, bool isLastAvailableTicket)
        {
            timeStart = _max_value;
            windowId = 0;

            if (maps == null || maps.Count == 0 || interval <= 0)
                return false;

            int min = _max_value;

			foreach (LineBusyMap map in maps)
            {
                int val = map.GetMinStartTime(interval);

                if (val < min)
                    min = val;
            }

            if (min == _max_value)
                return false;
            else
            {
                timeStart = min;

                // Если остался один талон и ему для записи не хватает одной минуты из-за особенностей составления карт занятости, то выдаём его всё равно
				if (isLastAvailableTicket && LineBusyMap.GetEndTime(maps) - timeStart - interval == -1)
                    interval--;

                return RegisterInterval(maps, min, interval + min, out windowId);
            }
        }

        /// <summary>
        /// Зарегистрировать время для талона
        /// </summary>
        /// <param name="maps">Список карт</param>
        /// <param name="timeStart">Время начала записи</param>
        /// <param name="timeEnd">Время окончания записи</param>
        /// <returns>true - записан, false - отказ</returns>
		public static bool RegisterInterval(IList<LineBusyMap> maps, int timeStart, int timeEnd, out long windowId)
        {
            windowId = 0;
            if (maps == null || maps.Count == 0)
                return false;
            if (timeEnd <= timeStart || timeEnd <= 0)
                return false;

			foreach (LineBusyMap map in maps)
                if (map.IsOpenInterval(timeStart, timeEnd))
                {
                    windowId = map.ResourceId;
                    map.BusyLine(timeStart, timeEnd);
                    return true;
                }
            return false;
        }

        [DataMember]
        public static int MaxValue
        {
            get
            {
                return _max_value;
            }
        }

		public static int GetMinStartTime(int interval, IList<LineBusyMap> maps)
        {
            if (maps == null || maps.Count == 0)
                return MaxValue;
            int min = MaxValue;
			foreach (LineBusyMap map in maps)
            {
                int cur = map.GetMinStartTime(interval);
                if (cur < min)
                    min = cur;
            }
            return min;
        }

        /// <summary>
        /// Возвращает время начала приема для конкретной карты занятости
        /// </summary>
        public int GetStartTime()
        {
            for (int i = 0; i < _max_value; i++)
            {
                if (Line[i] != LineItemState.NoRecord)
                    return i;
            }

            return 0;
        }

        /// <summary>
        /// Возвращает время начала приема для списка карт занятости
        /// </summary>
		public static int GetStartTime(IList<LineBusyMap> maps)
        {
            if (maps == null || maps.Count == 0)
                return 0;

            IList<int> times = new List<int>();

			foreach (LineBusyMap map in maps)
                times.Add(map.GetStartTime());

            return times.Min();
        }

        /// <summary>
        /// Возвращает время окончания приема для конкретной карты занятости
        /// </summary>
        public int GetEndTime()
        {
            for (int i = _max_value - 1; i >= 0 ; i--)
            {
                if (Line[i] != LineItemState.NoRecord)
                    return i;
            }

            return 0;
        }

        /// <summary>
        /// Возвращает время окончания приема для списка карт занятости
        /// </summary>
		public static int GetEndTime(IList<LineBusyMap> maps)
        {
            if (maps == null || maps.Count == 0)
                return 0;

            IList<int> times = new List<int>();

			foreach (LineBusyMap map in maps)
                times.Add(map.GetEndTime());

            return times.Max();
        }
    }

}
