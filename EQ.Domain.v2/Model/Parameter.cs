using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace EQ.Domain.v2
{
    
    /// <summary>
    /// Параметр системы
    /// </summary>
    public class Parameter
    {
        private string _value;

        public virtual long Id { get; protected set; }

        /// <summary>
        /// Наименование параметра для системы
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Наименование параметра для пользователя
        /// </summary>
        public virtual string Caption { get; protected set; }

        /// <summary>
        /// Подробное описание параметра
        /// </summary>
        public virtual string Remark { get; protected set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public virtual string Value 
        {
            get { return _value ?? string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(Regex) && !Util.CheckRegex(Regex, value))
                    throw new Exception(string.Format("Некорректно задан параметр {0}", Caption));
                    _value = value;
            }
        }

        /// <summary>
        /// Регулярное выражение для проверки вводимого значения
        /// </summary>
        public virtual string Regex { get; protected set; }

        /// <summary>
        /// Возможность переопределить в отделе (если false - параметр является общим и переопределить его значение в отделе нельзя)
        /// </summary>
        public virtual bool IsRedefining { get; protected set; }

        /// <summary>
        /// Список допустимых значений параметра
        /// </summary>
        public virtual string AllowedValues { get; protected set; }

        /// <summary>
        /// Тип данных для параметра
        /// </summary>
        public virtual ParameterDataType DataType { get; protected set; }
    }

    /// <summary>
    /// Тип данных для параметра
    /// </summary>
    public enum ParameterDataType
    {
        String = 0,
        Boolean = 1,
        Int = 2,
        Memo = 3
    }

    /// <summary>
    /// Параметр для отдела
    /// </summary>
    public class ParameterItem
    {
        private string _value;

        public virtual Parameter Parameter { get; protected set; }
        public virtual Department Department { get; protected set; }
        
        public virtual string Value
        {
            get { return _value ?? string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(Regex) && !Util.CheckRegex(Regex, value))
                    throw new Exception(string.Format("Некорректно задан параметр {0}", Caption));
                _value = value;
            }
        }

        public virtual string Name
        {
            get
            {
                return Parameter != null ? Parameter.Name : string.Empty;
            }
        }

        public virtual string Caption
        {
            get
            {
                return Parameter != null ? Parameter.Caption : string.Empty;
            }
        }

        public virtual string Remark
        {
            get
            {
                return Parameter != null ? Parameter.Remark : string.Empty;
            }
        }

        public virtual string Regex
        {
            get
            {
                return Parameter != null ? Parameter.Regex : string.Empty;
            }
        }

        public virtual string AllowedValues
        {
            get
            {
                return Parameter != null ? Parameter.AllowedValues : string.Empty;
            }
        }  
    }

}
