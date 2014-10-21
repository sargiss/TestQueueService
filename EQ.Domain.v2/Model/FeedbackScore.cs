using System;

namespace EQ.Domain.v2
{
	public class FeedbackScore
	{
		public virtual long Id { get; protected set; }
		public virtual Feedback Feedback { get; set; }
		public virtual FeedbackCriterion Criterion { get; set; }
		public virtual byte Score { get; set; }
        public virtual DateTime Inserted { get; set; }
        public FeedbackScore()
        {
            Inserted = DateTime.Now;
        }
	}
}