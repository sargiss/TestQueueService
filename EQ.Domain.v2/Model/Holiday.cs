using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Праздничные дни
    /// </summary>
    public class Holiday
    {
        private DateTime start;
        private DateTime end;

        public virtual long Id { get; protected set; }

        /// <summary>
        /// Наименование праздничного дня
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Дата начала
        /// </summary>
        public virtual DateTime DateStart 
        {
            get
            {
                return start;
            }
            set
            {
                if (end < value)
                    end = value;
                start = value;
            }
        }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public virtual DateTime DateEnd 
        {
            get
            {
                return end;
            }
            set
            {
                if (start > value)
                    end = start;
                else
                    end = value;
            }
        }

        public virtual bool IsIncludeDate(DateTime date)
        {
            return date >= DateStart && date <= DateEnd;
        }
    }


}

    
