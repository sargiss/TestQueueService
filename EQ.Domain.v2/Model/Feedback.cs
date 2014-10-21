using System;

namespace EQ.Domain.v2
{
	public class Feedback
	{
		public virtual long Id { get; protected set; }
		public virtual Department Department { get; set; }
		public virtual string ApplicantName { get; set; }
		public virtual DateTime Inserted { get; set; }
        public virtual string Comment { get; set; }

		public Feedback()
		{
			Inserted = DateTime.Now;
		}
	}
}