using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using EQ.Common.Extensions;
using EQ.Core.Interface;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Табло
    /// </summary>
    public abstract class Tablo
    {
		/// <summary>Список связанных окон</summary>
		public virtual IList<TabloWindow> Windows { get; set; }

        public virtual long Id { get; protected set; }

        public virtual string Name { get; set; }

        public virtual Department Department { get; set; }

        public virtual string Address { get; set; }

        public virtual string Key { get; set; }

		public virtual TabloType TabloType { get; protected set; }

        public abstract string TabloTypeName { get; set; }

		public virtual byte[] SettingXml { get; set; }

        public Tablo()
        {
            Windows = new List<TabloWindow>();
        }

		/// <summary>
		/// Добавление окна в список связанных окон, если окно с тем же Id не присутствует
		/// </summary>
        public virtual bool AddWindow(Window window, short deviceId = 0)
		{
			if (Windows.Any(w => w.Window.Id == window.Id))
				return false;
			
			Windows.Add(new TabloWindow(this, window) { DeviceId = deviceId });
            return true;
        }

		/// <summary>
		/// Удаление окна из списка связанных окон
		/// </summary>
        public virtual bool RemoveWindow(Window window)
		{
			int index = Windows.FirstIndexOf(x => x.Window.Id == window.Id);
			if (index == -1)
				return false;

			Windows.RemoveAt(index);
            return true;
        }
    }

    [Serializable]
    public enum TabloType
    {
        Unknown = 0,
        CommonLcd = 10,
        CommonLed = 11,
        Led = 20,
        QualityPad = 30
    }

	public enum TabloAssignmentType
	{
		None = 0,
		Queues = 10,
		Windows = 20,
	}

    /// <summary>
    /// Связь табло и назначенного окна
    /// </summary>
    public class TabloWindow
    {
        /// <summary>
        /// Табло (TABLO_ID)
        /// </summary>
        public virtual Tablo Tablo { get; protected set; }

        /// <summary>
        /// Назначенное окно (WINDOW_ID)
        /// </summary>
        public virtual Window Window { get; set; }

        /// <summary>
        /// Идентификатор окна
        /// </summary>
        public virtual long WindowId { get { return Window != null ? Window.Id : 0; } }

        /// <summary>
        /// Внутренний идентификатор устройства (DEVICE_ID)
        /// </summary>
        public virtual short DeviceId { get; set; }

        protected TabloWindow()
        {
        }

        public TabloWindow(Tablo tablo, Window window)
        {
            Tablo = tablo;
            Window = window;
        }

        public override bool Equals(object obj)
        {
            return ((TabloWindow)obj).GetHashCode() == this.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(Window.Id);
        }
    }

    /// <summary>
    /// Индивидуальное информационное табло
    /// </summary>
    public class IndividualTablo : Tablo
    {
		public IndividualTablo() { TabloType = TabloType.Led; }

        public override string TabloTypeName { get { return "Индивидуальное светодиодное"; } set { } }
    }

    /// <summary>
    /// Пульт оценки качества обслуживания
    /// </summary>
    public class QualityPad : Tablo
    {
		public QualityPad() { TabloType = TabloType.QualityPad; }

        public override string TabloTypeName { get { return "Пульт оценки качества обслуживания"; } set { } }
    }

    /// <summary>
    /// Общее информационное табло
    /// </summary>
    public abstract class CommonTablo : Tablo
    {
		public virtual IList<Queue> Queues { get; set; }

        public CommonTablo()
        {
	        Queues = new List<Queue>();
        }

	    public abstract void SaveSetting();

		public virtual TabloAssignmentType AssignmentType
	    {
		    get
		    {
			    if (Queues.Count > 0)
					return TabloAssignmentType.Queues;

				if (Windows.Count > 0)
					return TabloAssignmentType.Windows;

			    return TabloAssignmentType.None;
		    }
	    }

		/// <summary>
		/// Добавление очереди в список связанных очередей, если очередь с тем же Id не присутствует
		/// </summary>
		public virtual bool AddQueue(Queue queue)
		{
			if (Queues.Any(x => x.Id == queue.Id))
				return false;

			Queues.Add(queue);
			return true;
		}

		/// <summary>
		/// Удаление очереди из списка связанных очередей
		/// </summary>
		public virtual bool RemoveQueue(Queue queue)
		{
			int index = Queues.FirstIndexOf(x => x.Id == queue.Id);
			if (index == -1)
				return false;

			Queues.RemoveAt(index);
			return true;
		}
    }

    /// <summary>
    /// Общее информационное табло (жидкокристаллическое)
    /// </summary>
    public class CommonLcdTablo : CommonTablo
    {
		private CommonLcdTabloSetting cachedSetting;

        public CommonLcdTablo()
        {
			TabloType = TabloType.CommonLcd;
        }

        public override string TabloTypeName { get { return "Общее жидкокристаллическое"; } set { } }

		/// <summary>
		/// Десериализованные настройки табло. Изменения в данном экземпляре не сериализуются
		/// обратно автоматически. Для сериализации необходимо вызвать <see cref="SaveSetting"/>.
		/// </summary>
        public virtual CommonLcdTabloSetting Setting
        {
			get
			{
				if (cachedSetting != null)
					return cachedSetting;

				if (SettingXml == null)
					return cachedSetting = CommonLcdTabloSetting.GetDefault();

				return cachedSetting = SettingXml.Deserialize<CommonLcdTabloSetting>();
			}
			set { cachedSetting = value; }
        }

		/// <summary>
		/// Сериализует настройки в XML для сохранения в БД
		/// </summary>
		public override void SaveSetting()
		{
			if (cachedSetting != null)
				SettingXml = cachedSetting.Serialize();
		}
    }

    /// <summary>
    /// Общее информационное табло (составное из светодиодных панелей)
    /// </summary>
    public class CommonLedTablo : CommonTablo
    {
	    private CommonLedTabloSetting cachedSetting;

        public CommonLedTablo()
        {
			TabloType = TabloType.CommonLed;
        }

        public override string TabloTypeName { get { return "Общее светодиодное"; } set { } }

		/// <summary>
		/// Десериализованные настройки табло. Изменения в данном экземпляре не сериализуются
		/// обратно автоматически. Для сериализации необходимо вызвать <see cref="SaveSetting"/>.
		/// </summary>
		public virtual CommonLedTabloSetting Setting
		{
			get
			{
				if (cachedSetting != null)
					return cachedSetting;

				if (SettingXml == null)
					return cachedSetting = CommonLedTabloSetting.GetDefault();

				return cachedSetting = SettingXml.Deserialize<CommonLedTabloSetting>();
			}
			set { cachedSetting = value; }
		}

		/// <summary>
		/// Сериализует настройки в XML для сохранения в БД
		/// </summary>
		public override void SaveSetting()
		{
			if (cachedSetting != null)
				SettingXml = cachedSetting.Serialize();
		}
    }
}

public static class Converters
{
    /// <summary>
    /// Десериализует массив байт в любой сериализуемый класс
    /// </summary>
    public static T Deserialize<T>(this byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            return (T)s.Deserialize(stream);
        }
    }

    /// <summary>
    /// Сериализует любой сериализуемый класс в массив байт
    /// </summary>
    public static byte[] Serialize<T>(this T target)
    {
        using (MemoryStream stream = new MemoryStream())
        using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = false }))
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            s.Serialize(writer, target, ns);
            writer.Flush();
            stream.Position = 0;
            return stream.ToArray();
        }
    }
}