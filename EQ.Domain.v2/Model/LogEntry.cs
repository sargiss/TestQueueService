using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Запись в журнале изменений
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Дата изменения
        /// </summary>
        public virtual DateTime Inserted { get; set; }

        /// <summary>
        /// Отдел
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// IP адрес пользователя
        /// </summary>
        public virtual string Ip { get; set; }

        /// <summary>
        /// Тип совершённого действия
        /// </summary>
        public virtual ActionType ActionType { get; set; }

        /// <summary>
        /// Раздел административного интерфейса
        /// </summary>
        public virtual string Section { get; set; }

        /// <summary>
        /// Подробное описание совершённого действия
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// Наименование отдела
        /// </summary>
        public virtual string DepartmentName
        {
            get
            {
                if (Department != null)
                    return Department.Name;
                return string.Empty; 
            }
        }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public virtual string UserName
        {
            get
            {
                if (User != null)
                    return User.Name;
                return string.Empty;
            }
        }

        /// <summary>
        /// Описание типа совершённого действия
        /// </summary>
        public virtual string ActionName
        {
            get
            {
                switch ((ActionType))
                {
                    case ActionType.Insert: return "Вставка";
                    case ActionType.Update: return "Изменение";
                    case ActionType.Delete: return "Удаление";
                    default: return string.Empty;                        
                }
            }
        }
    }

    /// <summary>
    /// Тип совершённого действия
    /// </summary>
    public enum ActionType
    {
        [Display(Name = "Неизвестно")]
        Unknown,

        [Display(Name = "Вставка")]
        Insert,

        [Display(Name = "Изменение")]
        Update,

        [Display(Name = "Удаление")]
        Delete
    }
}
