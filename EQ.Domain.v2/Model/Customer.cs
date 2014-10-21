using System;
using System.ComponentModel.DataAnnotations;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Заявитель опроса Заявителя
    /// </summary>
    public class Customer
    {
        [Required]
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Отдел
        /// </summary>
        [Required]        
        public virtual Department Department { get; set; }

        /// <summary>
        /// ФИО заявителя
        /// </summary>
        [StringLength(100)]
        public virtual string Name { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Required]                
        public virtual DateTime Inserted { get; set; }

        public Customer()
        {
            Inserted = DateTime.Now;
        }
    }
}
