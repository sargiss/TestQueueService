namespace EQ.Domain.v2
{
    /// <summary>
    /// Значения параметров выбора для талона
    /// </summary>
    public class TicketOptionItem
    {
        public virtual OptionSettingValue OptionSettingValue { get; protected set; }

        public virtual long Id { get; protected set; }

		protected Ticket Ticket { get; private set; }

        public TicketOptionItem() { }

        internal TicketOptionItem(Ticket ticket, OptionSettingValue optionSettingValue)
        {
			Ticket = ticket;
            OptionSettingValue = optionSettingValue;
        }
    }
}
