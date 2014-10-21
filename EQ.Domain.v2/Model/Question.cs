using System;
using System.ComponentModel.DataAnnotations;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Вопрос системы опроса Заявителя
    /// </summary>
    public class Question
    {
        [Required]
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Отдел
        /// </summary>
        [Required]        
        public virtual Department Department { get; set; }

        /// <summary>
        /// Порядковый номер вопроса
        /// </summary>
        [Required]
        public virtual short Sequence { get; set; }

        /// <summary>
        /// Текст вопроса
        /// </summary>
        [StringLength(500)]
        public virtual string Text { get; set; }

        /// <summary>
        /// Отображать/скрывать вопрос
        /// </summary>
        [Required]
        public virtual bool IsShow { get; set; }

        /// <summary>
        /// Дата начала действия
        /// </summary>
        [Required]
        public virtual DateTime Inserted { get; set; }

        /// <summary>
        /// Дата окончания действия
        /// </summary>
		public virtual DateTime? Deleted { get; set; }

        public Question()
        {
            Inserted = DateTime.Now;
            IsShow = true;
        }
    }
}
