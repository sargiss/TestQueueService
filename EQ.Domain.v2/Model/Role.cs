using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Роли
    /// </summary>
    public class Role
    {
        public virtual long Id { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string Caption { get; protected set; }
        public virtual string Remark { get; protected set; }

        /// <summary>
        /// Возможность назначить роль
        /// </summary>
        public virtual bool IsGrant { get; protected set; }

        internal Role()
        { }
    }

    /// <summary>
    /// Дополнительная функция, которая может быть назначена пользователю
    /// </summary>
    public class Feature
    {
        public virtual long Id { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string Caption { get; protected set; }
        public virtual string Remark { get; protected set; }
        public virtual Role Role { get; protected set; }

        public virtual FeatureEnum Type
        {
            get
            {
                if (Enum.IsDefined(typeof(FeatureEnum), Id))
                    return (FeatureEnum)Id;
                return FeatureEnum.Unknown;
            }
        }
        internal Feature()
        { }
    }
}
