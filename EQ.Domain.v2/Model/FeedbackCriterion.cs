using System;

namespace EQ.Domain.v2
{
	public class FeedbackCriterion
	{
		public virtual long Id { get; protected set; }
		public virtual Department Department { get; set; }
		public virtual string Name { get; set; }
		public virtual short Sequence { get; set; }
		public virtual bool Enabled { get; set; }
		public virtual DateTime Inserted { get; set; }
		public virtual DateTime? Deleted { get; set; }

        public FeedbackCriterion()
		{
			Inserted = DateTime.Now;
		}
	}
}