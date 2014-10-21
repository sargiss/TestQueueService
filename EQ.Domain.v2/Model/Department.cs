using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Отдел
    /// </summary>
    public class Department
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Идентификатор отдела на портале Росреестра
        /// </summary>
        public virtual string PortalId { get; set; }

		/// <summary>
		/// Идентификатор отдела в учетной системе
		/// </summary>
		public virtual long? AccountSystemId { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Родительский отдел
        /// </summary>
        public virtual Department Parent { get; set; }

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

        /// <summary>
        /// Наименование головного офиса (Управления или палаты, к которому относится офис)
        /// </summary>
        public virtual string HeadOffice { get; set; }

        /// <summary>
        /// Тип отдела (подразделение, к которому принадлежит отдел)
        /// </summary>
        public virtual Division DepartmentDivision { get; set; }

        public virtual long? DivisionId { get; set; }

        public virtual Address DepartmentAddress { get; set; }

        /// <summary>
        /// Дочерние отделы
        /// </summary>
        public virtual IList<Department> Childs { get; set; }

		public virtual IList<Queue> QueueList { get; set; }
        
		public virtual IList<Window> Windows { get; set; }
        
		public virtual IList<ParameterItem> Parameters { get; set; }

		public virtual IList<Questionnaire> Questionnaires { get; set; }

        public virtual string GetParameterValue(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
			foreach (ParameterItem p in Parameters)
                if (p.Name.ToLower() == name.ToLower())
                    return p.Value;
            return string.Empty;
        }

        public virtual bool HasParent(Department department)
        {
            if (Parent == null)
                return false;
            if (Parent != null && department != null && Parent.Id == department.Id)
                return true;
            else
                return Parent.HasParent(department);
        }

        public virtual IList<PageParameter> PageParams { get; set; }

        public Department()
        {
            Childs = new List<Department>();
            QueueList = new List<Queue>();
            Windows = new List<Window>();
			Parameters = new List<ParameterItem>();
            PageParams = new List<PageParameter>();
	        Questionnaires = new List<Questionnaire>();
        }
    }
}