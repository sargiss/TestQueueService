using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Источник формирования талона
    /// </summary>
    public class Source
    {
        public virtual long Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual string Remark { get; set; }
    }

}
