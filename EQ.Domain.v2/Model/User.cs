using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace EQ.Domain.v2
{
    public enum FeatureEnum : long
    {
        Unknown = 0,

        /// <summary>
        /// Разрешить клонировать отделы
        /// </summary>
		[Display(Name = "Разрешить клонировать отделы")]
        CloneDepartment = 1001,

        /// <summary>
        /// Разрешить доступ к странице настройки праздничных дней
        /// </summary>
		[Display(Name = "Разрешить доступ к странице настройки праздничных дней")]
        AddHoliday = 1002,

        /// <summary>
        /// Разрешить пропускать талоны
        /// </summary>
		[Display(Name = "Разрешить пропускать талоны")]
		IgnoreTicket = 5001,

        /// <summary>
        /// Разрешить откладывать талон (для приема позднее)
        /// </summary>
		[Display(Name = "Разрешить откладывать талон (для приема позднее)")]
		DeferTicket = 5002,

        /// <summary>
        /// Разрешить перенаправлять клиента в другую очередь (окно)
        /// </summary>
		[Display(Name = "Разрешить перенаправлять клиента в другую очередь (окно)")]
		RedirectTicket = 5003,

        /// <summary>
        /// Разрешить повторный вызов клиента из очереди
        /// </summary>
		[Display(Name = "Разрешить повторный вызов клиента из очереди")]
		RecallTicket = 5004,

        /// <summary>
        /// Разрешить добавлять дополнительный талон (через операторское рабочее место)
        /// </summary>
		[Display(Name = "Разрешить добавлять дополнительный талон (через операторское рабочее место)")]
		InsertAdditionalTicket = 5005
    }

    public enum RoleEnum : long
    {
        Unknown = 0,
        
        /// <summary>
        /// Глобальный администратор (полные права на все)
        /// </summary>
        GlobalAdmin = 1,

        /// <summary>
        /// Администратор системы
        /// </summary>
        Admin = 10,

        /// <summary>
        /// Консультант (запись клиентов в очередь)
        /// </summary>
        Consultant = 20,

        /// <summary>
        /// Управление очередью 
        /// </summary>
        QueueManager = 30,
        
        /// <summary>
        /// Просмотр отчетов
        /// </summary>
        ReportViewer = 40,
        
        /// <summary>
        /// Обработка талонов (рабочее место оператора)
        /// </summary>
        Operator = 50
    }

    /// <summary>
    /// Пользователи системы (операторы, администраторы и т.д)
    /// </summary>
    public class User 
    {
        private string _password;

        public virtual long Id { get; protected set; }

        public virtual string Name { get; set; }
        public virtual string Login { get; set; }
        
        public virtual Department Department { get; set; }
        public virtual bool Lock { get; set; }

	    protected virtual string Password
	    {
		    get { return _password; }
		    set { _password = value; }
	    }

	    public virtual IList<RoleItem> RoleItems { get; set; }

		public virtual IList<Feature> FeatureItems { get; protected set; }

        public virtual ReadOnlyCollection<FeatureEnum> Features
        {
            get
            {
                IList<FeatureEnum> fe = new List<FeatureEnum>();
				foreach (Feature f in FeatureItems)
                    if (!fe.Contains(f.Type) && f.Type != FeatureEnum.Unknown)
                        fe.Add(f.Type);
                return new ReadOnlyCollection<FeatureEnum>(fe);
            }
        }

        public virtual string RolesName
        {
            get
            {
                var sb = new StringBuilder();
                foreach (RoleItem role in RoleItems)
                    sb.AppendFormat("- {0} {1}{2}", role.RoleCaption, role.IsHierarchical ? "(*доступ к дочерним объектам)" : String.Empty, Environment.NewLine);

                if (FeatureItems.Any())
                {
                    sb.AppendLine("");
                    sb.AppendLine("Дополнительные возможности:");

                    foreach (Feature feature in FeatureItems)
                        sb.AppendFormat("- {0}{1}",feature.Caption, Environment.NewLine);
                }

                return sb.ToString();
            }
        }

        public User()
        {
            RoleItems = new List<RoleItem>();
			FeatureItems = new List<Feature>();
        }

        public virtual bool AddRole(Role role, Department department, bool isHierarchical)
        {
            if (role == null)
                return false;
			foreach (RoleItem r in RoleItems)
                if (r.Role.Id == role.Id) //такая роль уже определена
                    if ((r.Department == null && department == null) || (r.Department != null && department != null && r.Department.Id == department.Id))
                        return false;
            if (role.Name != "global_admin" && department == null)
                return false;
            RoleItem ri = RoleItem.New(this, role, department, isHierarchical);
			RoleItems.Add(ri);
            return true;
        }

        public virtual bool DeleteRole(RoleItem roleItem)
        {
            if (roleItem == null)
                return false;
			if (RoleItems.Contains(roleItem))
            {
				RoleItems.Remove(roleItem);

                List<Feature> del_feature = new List<Feature>();
				foreach (Feature f in FeatureItems)
                    if (f.Role.Id == roleItem.Role.Id)
                        del_feature.Add(f);
                if (del_feature.Count > 0)
                    foreach (Feature f in del_feature)
						if (FeatureItems.Contains(f))
							FeatureItems.Remove(f);
                    
                return true;
            }
            return false;
        }

        public virtual bool IsInRole(RoleEnum role, Department department)
        {
			foreach (RoleItem ri in RoleItems)
                if (ri.RoleType == role)
                {   
                    if (ri.Department == null && department == null)
                        return true;
                    if (ri.Department != null && department != null)
                    {
                        if (ri.Department.Id == department.Id)
                            return true;
                        if (department.HasParent(ri.Department))
                            return true;
                    }
                }
            return false;
        }

        public virtual bool IsInRole(RoleEnum role)
        {
            return RoleItems.Any(ri => ri.RoleType == role);
        }


        public virtual bool AddFeature(Feature feature)
        {
            if (feature == null)
                return false;
			if (!FeatureItems.Contains(feature))
            {
				FeatureItems.Add(feature);
                return true;
            }
            return false;
        }

        public virtual bool DeleteFeature(Feature feature)
        {
            if (feature == null)
                return false;
			if (FeatureItems.Contains(feature))
            {
				FeatureItems.Remove(feature);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Создать пользователя
        /// </summary>
        public static User Create(string name, string login, string password, Department dept)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Не определено ФИО пользователя");
            if (string.IsNullOrEmpty(login))
                throw new Exception("За определен Логин для входа в систему");
            if (string.IsNullOrEmpty(password))
                throw new Exception("Не задан пароль");

            User usr = new User();
            usr.Name = name;
            usr.Lock = false;
            usr.Login = login.Trim();
            string pass = string.Concat(login.Trim(), password.Trim());
            usr._password = HashManager.MD5Hash(pass);
            usr.Department = dept;
            return usr;
        }

        /// <summary>
        /// Проверить пароль пользователя
        /// </summary>
        public virtual bool CheckPass(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            
            string pass = string.Concat(Login, password.Trim());
            return _password == HashManager.MD5Hash(pass);
        }

        /// <summary>
        /// Сменить пароль пользователя
        /// </summary>
        public virtual bool ChangePass(string newpass)
        {
            if (string.IsNullOrEmpty(newpass))
                return false;
            string pass = string.Concat(Login, newpass.Trim());
            _password = HashManager.MD5Hash(pass);
            return true;
        }
    }

    public class RoleItem
    {
        public virtual long Id { get; protected set; }
        public virtual User User { get; protected set; }
        public virtual Role Role { get; protected set; }
        public virtual Department Department { get; protected set; }
        public virtual bool IsHierarchical { get; set; }

        public virtual RoleEnum RoleType
        {
            get
            {
                if (Role != null)
                    if (Enum.IsDefined(typeof(RoleEnum), Role.Id))
                        return (RoleEnum)Role.Id;
                return RoleEnum.Unknown;
            }
        }

        public virtual string RoleCaption
        {
            get
            {
                if (Role != null)
                    return Role.Caption;

                return string.Empty;
            }
        }

        internal RoleItem()
        { }

        public static RoleItem New(User user, Role role, Department department, bool isHierarchical)
        {
            return new RoleItem { User = user, Role = role, Department = department, IsHierarchical = isHierarchical };
        }
    }


    
}
