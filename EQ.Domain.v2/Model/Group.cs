using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Категория очереди
    /// </summary>
    public class Group
    {
        //Идентификатор
        public virtual long Id { get; protected set; }
        /// <summary>
        /// Наименование категории
        /// </summary>
        public virtual string Name { get; protected set; }
        
        //Родительская категория
        public virtual Group Parent { get; protected set; }
    }

}
