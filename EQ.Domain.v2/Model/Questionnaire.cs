using System;

namespace EQ.Domain.v2
{
    /// <summary>
    /// Опросный лист
    /// </summary>
    public class Questionnaire
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public virtual long Id { get; protected set; }

        /// <summary>
        /// Отдел
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// ФИО отвечавшего
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Версия опросного листа
        /// </summary>
        public virtual int Version { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public virtual DateTime Inserted { get; set; }

		protected virtual byte[] AnswersSerialized { get; set; }

        /// <summary>
        /// Ответы
        /// </summary>
        public virtual QuestionList Answers
        {
			get { return AnswersSerialized == null ? new QuestionList() : AnswersSerialized.Deserialize<QuestionList>(); }
			set { AnswersSerialized = value.Serialize(); }
        }
    }

    /// <summary>
    /// Список вопросов
    /// </summary>
    [Serializable]
    public struct QuestionList
    {
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Цель обращения
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Качество организации записи
        /// </summary>
        public string RecordOrganization { get; set; }

        /// <summary>
        /// Время ожидания
        /// </summary>
        public string WaitingTime { get; set; }

        /// <summary>
        /// Качество организации офиса приёма
        /// </summary>
        public string OfficeOrganization { get; set; }

        /// <summary>
        /// Качество организации места ожидания приёма
        /// </summary>
        public string WaitingOrganization { get; set; }

        /// <summary>
        /// Время обслуживания
        /// </summary>
        public string ProcessingTime { get; set; }

        /// <summary>
        /// Факт вымогательства
        /// </summary>
        public bool Extortion { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// ФИО отвечавшего
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Соответствие графика работы реальному расписанию (по пятибальной шкале)
        /// </summary>
        public int WorkHours { get; set; }

        /// <summary>
        /// Точность и достоверность ответов сотрудников на задаваемые Вами вопросы (по пятибальной шкале)
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// Корректность сотрудников (по пятибальной шкале)
        /// </summary>
        public int Politeness { get; set; }

        /// <summary>
        /// Профессионализм сотрудников (по пятибальной шкале)
        /// </summary>
        public int Profficiency { get; set; }

        /// <summary>
        /// Организация работы с заявителям (по пятибальной шкале)
        /// </summary>
        public int Organization { get; set; }
    }
}
