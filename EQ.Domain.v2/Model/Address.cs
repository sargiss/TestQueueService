using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQ.Domain.v2
{
    public class Address
    {
        public virtual long Id { get; set; }
        public virtual string Region { get; set; }
        public virtual string District { get; set; }
        public virtual string City { get; set; }
        public virtual string Street { get; set; }

        public override string ToString()
        {
            List<string> parts = new List<string>();

            AddPart(Region, parts);
            AddPart(District, parts);
            AddPart(City, parts);
            AddPart(Street, parts);

            return string.Join(", ", parts);
        }

        private void AddPart(string part, List<string> list)
        {
            if (!string.IsNullOrEmpty(part))
                list.Add(part);
        }
    }
}
