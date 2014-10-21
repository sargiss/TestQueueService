using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Параметр выбора из нескольких вариантов
    /// </summary>
    public class OptionSetting
    {
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Наименование параметра
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Список значений параметра выбора
        /// </summary>
		public virtual IList<OptionSettingValue> OptionSettingValues { get; set; }

        public virtual bool RemoveOptionSettingValue(OptionSettingValue elem)
        {
			if (OptionSettingValues.Contains(elem))
            {
				OptionSettingValues.Remove(elem);
                return true;
            }

            return false;
        }

		public virtual Queue Queue { get; protected set; }

        protected OptionSetting() { }

        public OptionSetting(Queue queue)
        {
			Queue = queue;
			OptionSettingValues = new List<OptionSettingValue>();
        }

		public virtual OptionSetting Clone(Queue queue)
		{
			OptionSetting clone = (OptionSetting)MemberwiseClone();
			clone.Id = 0;
			clone.Queue = queue;
			clone.OptionSettingValues = OptionSettingValues.Select(x => x.Clone(clone)).ToList();
			return clone;
		}
    }

    /// <summary>
    /// Значение (вариант выбора) параметра выбора
    /// </summary>
    public class OptionSettingValue
    {
        public virtual OptionSetting OptionSetting { get; protected set; }

        public virtual long Id { get; protected set; }

        /// <summary>
        /// Наименование значения
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Добавляемое время приёма в минутах
        /// </summary>
        public virtual int AdditionalTime { get; set; }

        /// <summary>
        /// Признак приоритетности при приёме
        /// </summary>
        public virtual bool IsPreferential { get; set; }

        protected OptionSettingValue() { }

        public OptionSettingValue(OptionSetting optionSetting)
        {
            OptionSetting = optionSetting;
        }

		public virtual OptionSettingValue Clone(OptionSetting optionSetting)
		{
			OptionSettingValue clone = (OptionSettingValue)MemberwiseClone();
			clone.Id = 0;
			clone.OptionSetting = optionSetting;
			return clone;
		}
    }
}