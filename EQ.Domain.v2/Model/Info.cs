using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Страница
    /// </summary>
    public class Page
    {
        private byte[] _value;

        public virtual long Id { get; protected set; }

        public virtual string Name { get; set; }

        public virtual string Caption { get; set; }

        public virtual Category Category { get; set; }

        public virtual string CategoryName
        {
            get
            {
                if (Category != null)
                    return Category.Name;
                else
                    return string.Empty;
            }
        }

        public virtual string Value
        {
            get
            {
                if (_value == null)
                    return string.Empty;
                try
                {
                    return Encoding.UTF8.GetString(_value);
                }
                catch 
                {
                    return string.Empty;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _value = null;
                try
                {
                    _value = Encoding.UTF8.GetBytes(value);
                }
                catch
                {
                    _value = null;
                }
            }
        }

    }


    /// <summary>
    /// Категория, для страниц (справочной информации)
    /// </summary>
    public class Category
    {
        public virtual long Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual Category Parent { get; set; }
		public virtual IList<Category> ChildCategories { get; protected set; }

        /// <summary>
        /// Код родительской очереди
        /// </summary>
        public virtual long ParentId
        {
            get
            {
                if (Parent == null)
                    return 0;
                else
                    return Parent.Id;
            }
        }

		public Category()
		{
			ChildCategories = new List<Category>();
		}
    }

    /// <summary>
    /// Параметр на странице
    /// </summary>
    public class PageParameter
    {
        private byte[] _value;

        public virtual long Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual string Caption { get; set; }
        
        public virtual Department Department { get; protected set; }

        public virtual string Value 
        {
            get
            {
                if (_value == null)
                    return string.Empty;
                try
                {
                    return Encoding.UTF8.GetString(_value);
                }
                catch
                {
                    return string.Empty;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _value = null;
                try
                {
                    _value = Encoding.UTF8.GetBytes(value);
                }
                catch
                {
                    _value = null;
                }
            }
        }

        public PageParameter()
        { }

        public PageParameter(Department department)
        {
            Department = department;
        }

    }

    /// <summary>
    /// Заявляемое действие справочной информации
    /// </summary>
    public class ActionVoc
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Номер
        /// </summary>
        public virtual short Num { get; set; }

        /// <summary>
        /// Текст вопроса
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Родительское действие
        /// </summary>
        public virtual ActionVoc ParentActionVoc { get; set; }
    }

    /// <summary>
    /// Идентификатор заявляемого действия справочной информации
    /// </summary>
    public class ActionLink
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Тип объекта недвижимости
        /// </summary>
        public virtual short? RealtyType { get; set; }

        /// <summary>
        /// Заявляемое действие
        /// </summary>
        public virtual ActionVoc ActionVoc { get; set; }
    }

    /// <summary>
    /// Документ справочной информации
    /// </summary>
    public class DocumentVoc
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Номер
        /// </summary>
        public virtual short Num { get; set; }

        /// <summary>
        /// Текст вопроса
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Родительское действие
        /// </summary>
        public virtual DocumentVoc ParentDocumentVoc { get; set; }
    }

    /// <summary>
    /// Идентификатор документа справочной информации
    /// </summary>
    public class DocumentLink
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Заявляемое действие
        /// </summary>
        public virtual ActionLink ActionLink { get; set; }

        /// <summary>
        /// Документ
        /// </summary>
        public virtual DocumentVoc DocumentVoc { get; set; }

        /// <summary>
        /// Условие применения
        /// </summary>
        public virtual Condition Condition { get; set; }
    }

    /// <summary>
    /// Условие применения
    /// </summary>
    public class Condition
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Наименование условия
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Заявляемое действие
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Заголовок
        /// </summary>
        public virtual string Header { get; set; }
    }
}
