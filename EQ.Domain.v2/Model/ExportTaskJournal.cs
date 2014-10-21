using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQ.Domain.v2
{
    public class ExportTaskJournal
    {
        public ExportTaskJournal()
        {
            Details = new List<ExportDetailedJournal>();
        }
        public virtual long Id { get; set; }
        public virtual DateTime TaskLaunched { get; set; }
        public virtual DateTime? TaskFinished { get; set; }
        public virtual DateTime? DateFrom { get; set; }
        public virtual DateTime? DateTo { get; set; }
        public virtual bool Manual { get; set; }
        public virtual bool IsError { get; set; }
        public virtual string ErrorFileName { get; set; }
        public virtual IList<ExportDetailedJournal> Details { get; set; }
    }
}
