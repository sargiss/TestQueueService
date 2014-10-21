using System;
using System.ComponentModel.DataAnnotations;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Анкета системы опроса Заявителя
    /// </summary>
    public class Questionary
    {
        [Required]
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Заявитель
        /// </summary>
        [Required]
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Вопрос
        /// </summary>
        [Required]
        public virtual Question Question { get; set; }

        /// <summary>
        /// Текст ответа
        /// </summary>
        [StringLength(1000)]
        public virtual string Text { get; set; }

        /// <summary>
        /// Дата добавления
        /// </summary>
        [Required]
        public virtual DateTime Inserted { get; set; }

        public Questionary()
        {
            Inserted = DateTime.Now;
        }
    }
}
