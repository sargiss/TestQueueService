using System;
using System.Collections.Generic;

namespace EQ.Domain.v2
{
	/// <summary>
	/// Временной слот, в который доступна запись
	/// </summary>
	public class AvailableTimeSlot
	{
		/// <summary>Начало интервала</summary>
		public TimeSpan StartTime { get; set; }

		/// <summary>Окончание интервала</summary>
		public TimeSpan EndTime { get; set; }

		/// <summary>Минимальное время, на которое можно записаться в пределах данного интервала</summary>
		public TimeSpan MinRecordTime { get; set; }

		public AvailableTimeSlot(TimeSpan startTime, TimeSpan endTime, TimeSpan minRecordTime)
		{
			StartTime = startTime;
			EndTime = endTime;
			MinRecordTime = minRecordTime;
		}
	}

	/// <summary>
	/// Информация о доступности предварительной очереди для записи (на определенную дату)
	/// </summary>
	public class QueueAvailabilityInfo
	{
		/// <summary>
		/// Временные интервалы и минимальное время записи в рамках данного интервала:
		/// 
		/// Пример:
		/// SplitInteval = 30, WorkStartTime = 09:00
		/// => Рабочие часы очереди разбиваются на интервалы по 30 минут:
		/// 09:00 - 09:30, 09:30 - 10:00, ...
		/// 
		/// Имеем интервал вида AvailableTimeSlot { StartTime = 09:00, EndTime = 09:30, MinRecordTime = 09:15 }
		/// => Если клиент желает записаться в интервале 09:00 - 09:30, минимальное время, на которое он может записаться - 09:15
		/// </summary>
		public IList<AvailableTimeSlot> Intervals { get; set; }

		/// <summary>Время начала приёма</summary>
		public TimeSpan WorkStartTime { get; set; }

		/// <summary>Время окончания приёма</summary>
		public TimeSpan WorkEndTime { get; set; }

		/// <summary>Интервал между временными слотами</summary>
		public TimeSpan SplitInterval { get; set; }

		public QueueAvailabilityInfo()
		{
			Intervals = new List<AvailableTimeSlot>();
		}
	}
}