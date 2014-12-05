/*
    Copyright (c) 2010 250bpm s.r.o.
    Copyright (c) 2010-2011 Other contributors as noted in the AUTHORS file

    This file is part of 0MQ.

    0MQ is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    0MQ is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Linq;

namespace NetMQ
{
    public class Blob
    {
        private readonly byte[] m_buffer;
        private int m_hash = 0;

        public Blob(byte[] data, int size)
        {
            m_buffer = new byte[size];

            Buffer.BlockCopy(data, 0, m_buffer, 0, size);            
        }

        public Blob(int size)
        {
            m_buffer = new byte[size];
        }

        public Blob Put(int pos, byte b)
        {
            m_buffer[pos] = b;
            m_hash = 0;
            return this;
        }

        public Blob Put(int pos, byte[] data, int count)
        {
            Buffer.BlockCopy(data, 0, m_buffer, pos, count);

            m_hash = 0;
            return this;
        }

        public int Size
        {
            get { return m_buffer.Length; }
        }

        public byte[] Data
        {
            get { return m_buffer; }
        }

        public override bool Equals(Object t)
        {
            if (t is Blob)
            {
                Blob b = (Blob)t;
                if (b.m_buffer.Length != m_buffer.Length)
                {
                    return false;
                }

                return m_buffer.SequenceEqual(b.m_buffer);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (m_hash == 0)
            {
                foreach (byte b in m_buffer)
                {
                    m_hash = 31 * m_hash + b;
                }
            }
            return m_hash;
        }
    }
}
