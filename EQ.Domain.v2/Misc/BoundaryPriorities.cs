using System;
using System.Collections.Generic;
using System.Linq;

namespace EQ.Domain.v2
{
	/// <summary>
	/// Граничные значения приоритетов для заданного окна
	/// </summary>
	public class BoundaryPriorities
	{
		/// <summary>
		/// Назначены ли в окно живые очереди
		/// </summary>
		public bool HasLiveQueues { get; set; }

		/// <summary>
		/// Назначены ли в окно очереди по записи
		/// </summary>
		public bool HasPrerecordQueues { get; set; }

		/// <summary>
		/// Максимальный приоритет, заданный в данный момент живой очереди в данном окне
		/// </summary>
		public int MaxLivePriority { get; set; }

		/// <summary>
		/// Сколько живых очередей имеют максимальный на данный момент приоритет (соотвественно, одинаковый)
		/// </summary>
		public int MaxLivePriorityCount { get; set; }

		/// <summary>
		/// Минимальный приоритет, заданный в данный момент очереди по записи в данном окне
		/// </summary>
		public int MinPrerecordPriority { get; set; }

		/// <summary>
		/// Сколько очередей по записи имеют минимальный на данный момент приоритет (соотвественно, одинаковый)
		/// </summary>
		public int MinPrerecordPriorityCount { get; set; }
	}
}