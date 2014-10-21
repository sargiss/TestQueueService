using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Сведения о версии компонент
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Уникальное имя компонента
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Название компонента
        /// </summary>
        public virtual string Caption { get; protected set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public virtual string Remark { get; protected set; }

        /// <summary>
        /// Версия компонента
        /// </summary>
        public virtual string VersionNumber { get; protected set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public virtual DateTime Updated { get; protected set; }
    }
}
