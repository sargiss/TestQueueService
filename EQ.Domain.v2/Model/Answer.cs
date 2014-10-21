using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Вариант ответа на вопрос системы опроса Заявителя
    /// </summary>
    public class Answer
    {
        [Required]
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Вопрос
        /// </summary>
        [Required]
        public virtual Question Question { get; set; }

        /// <summary>
        /// Порядковый номер ответа
        /// </summary>
        [Required]
        public virtual short Sequence { get; set; }

        /// <summary>
        /// Текст ответа
        /// </summary>
        [StringLength(250)]
        public virtual string Text { get; set; }

        /// <summary>
        /// Отображать/скрывать ответ
        /// </summary>
        [Required]
        public virtual bool IsShow { get; set; }

        /// <summary>
        /// Тип поля ответа
        /// </summary>
        [Required]
        public virtual AnswerType Type { get; set; }

        /// <summary>
        /// Дата начала действия
        /// </summary>
        [Required]
        public virtual DateTime Inserted { get; set; }

        /// <summary>
        /// Дата окончания действия
        /// </summary>
		public virtual DateTime? Deleted { get; set; }

        public Answer()
        {
            Inserted = DateTime.Now;
            IsShow = true;
            Type = AnswerType.Simple;
        }
    }

    /// <summary>
    /// Тип поля ответа
    /// </summary>
    public enum AnswerType
    {
        /// <summary>Обычное поле</summary>
        [Display(Name = "Обычное поле")]
        Simple = 0,

        /// <summary>Связанное текстовое поле</summary>
        [Display(Name = "Связанное текстовое поле")]        
        Text = 1,

        /// <summary>Текстовое поле</summary>
        [Display(Name = "Текстовое поле")]
        TextArea = 2
    }
}
