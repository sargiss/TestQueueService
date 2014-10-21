using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQ.Domain.v2
{
    public class ExportDetailedJournal
    {
        public virtual long Id { get; set; }
        public virtual long TaskId { get; set; }
        public virtual long DepartmentId { get; set; }
        public virtual string FileName { get; set; }
        public virtual DateTime CompletedTime { get; set; }
        public virtual bool IsError { get; set; }
    }
}
