using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Common
{
    class ByteArrayComparer: IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x == y)
                return true;

            if (x != null && y != null)
            {
                return x.SequenceEqual(y);
            }

            return false;
        }

        public int GetHashCode(byte[] obj)
        {
            return obj.GetHashCode();
        }
    }
}
