using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Состояние талона
    /// </summary>
    public enum TicketStatus
    {
        Unknown = 0,
        /// <summary>
        /// Сформирован и Ожидает вызова
        /// </summary>
		[Display(Name = "Ожидает вызова")]
        Open = 10,

        /// <summary>
        /// Вызывается на прием
        /// </summary>
		[Display(Name = "Вызывается")]
		Call = 20,

        /// <summary>
        /// Вызов пропущен
        /// </summary>
		[Display(Name = "Пропущен")]
		Pass = 30,

        /// <summary>
        /// На приеме
        /// </summary>
		[Display(Name = "На приеме")]
		Process = 40,

        /// <summary>
        /// Отложен
        /// </summary>
		[Display(Name = "Отложен")]
		Pause = 50,

        /// <summary>
        /// Обработан и закрыт
        /// </summary>
		[Display(Name = "Обработан и закрыт")]
		Close = 60
    }

/*    /// <summary>
    /// Оценка качества обслуживания талона
    /// </summary>
    public enum ServiceQuality
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Очень плохое
        /// </summary>
        Worst = 20,

        /// <summary>
        /// Плохое
        /// </summary>
        Poor = 30,

        /// <summary>
        /// Хорошее
        /// </summary>
        Good = 40,

        /// <summary>
        /// Отличное
        /// </summary>
        Excellent = 50
    }*/

    internal class Util
    {
        public static bool CheckRegex(string regex, string value)
        {
            if (string.IsNullOrEmpty(regex))
                return true;
            Regex rx = new Regex(regex);
            Match m = rx.Match(value ?? string.Empty);
            return m.Success;
        }

        /// <summary>
        /// Формирует путь к очереди в иерархии
        /// </summary>
        /// <param name="queue">Очередь</param>
        /// <param name="name">Путь к очереди</param>
        /// <returns>Полный путь к очереди</returns>
        public static string BuildQueueFullPathName(Queue queue, ref string name)
        {
            if (queue.Parent != null)
            {
                name = string.Format("{0} -> {1}", queue.Parent.Name, name);
                BuildQueueFullPathName(queue.Parent, ref name);
            }

            return name;
        }
    }
}
