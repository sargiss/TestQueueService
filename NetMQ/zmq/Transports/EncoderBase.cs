/*
    Copyright (c) 2007-2012 iMatix Corporation
    Copyright (c) 2009-2011 250bpm s.r.o.
    Copyright (c) 2011 VMware, Inc.
    Copyright (c) 2007-2011 Other contributors as noted in the AUTHORS file

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

namespace NetMQ.zmq.Transports
{
    abstract class EncoderBase : IEncoder
    {
        //  Where to get the data to write from.    
        private ByteArraySegment m_writePos;

        //  If true, first byte of the message is being written.
        //    @SuppressWarnings("unused")
        private bool m_beginning;


        //  How much data to write before next step should be executed.
        private int m_toWrite;

        //  The buffer for encoded data.
        private readonly byte[] m_buf;

        private readonly int m_buffersize;


        private bool m_error;

        protected EncoderBase(int bufsize, Endianness endian)
        {
            Endian = endian;
            m_buffersize = bufsize;
            m_buf = new byte[bufsize];
            m_error = false;
        }

        public Endianness Endian { get; private set; }

        public abstract void SetMsgSource(IMsgSource msgSource);

        //  The function returns a batch of binary data. The data
        //  are filled to a supplied buffer. If no buffer is supplied (data_
        //  points to NULL) decoder object will provide buffer of its own.


        public void GetData(ref ByteArraySegment data, ref int size)
        {
            int offset = -1;

            GetData(ref data, ref size, ref offset);
        }

        public void GetData(ref ByteArraySegment data, ref int size, ref int offset)
        {
            ByteArraySegment buffer = data ?? new ByteArraySegment(m_buf);
            int bufferSize = data == null ? m_buffersize : size;

            int pos = 0;

            while (pos < bufferSize)
            {
                //  If there are no more data to return, run the state machine.
                //  If there are still no data, return what we already have
                //  in the buffer.
                if (m_toWrite == 0)
                {
                    //  If we are to encode the beginning of a new message,
                    //  adjust the message offset.

                    if (m_beginning)
                    {
                        if (offset == -1)
                        {
                            offset = pos;
                        }
                    }

                    if (!Next())
                        break;
                }

                //  If there are no data in the buffer yet and we are able to
                //  fill whole buffer in a single go, let's use zero-copy.
                //  There's no disadvantage to it as we cannot stuck multiple
                //  messages into the buffer anyway. Note that subsequent
                //  write(s) are non-blocking, thus each single write writes
                //  at most SO_SNDBUF bytes at once not depending on how large
                //  is the chunk returned from here.
                //  As a consequence, large messages being sent won't block
                //  other engines running in the same I/O thread for excessive
                //  amounts of time.
                if (pos == 0 && data == null && m_toWrite >= bufferSize)
                {
                    data = m_writePos;
                    size = m_toWrite;

                    m_writePos = null;
                    m_toWrite = 0;
                    return;
                }

                //  Copy data to the buffer. If the buffer is full, return.
                int toCopy = Math.Min(m_toWrite, bufferSize - pos);

                if (toCopy != 0)
                {
                    m_writePos.CopyTo(0, buffer, pos, toCopy);
                    pos += toCopy;
                    m_writePos.AdvanceOffset(toCopy);
                    m_toWrite -= toCopy;
                }
            }

            data = buffer;
            size = pos;
        }

        protected int State
        {
            get;
            private set;
        }

        protected void EncodingError()
        {
            m_error = true;
        }

        public bool IsError()
        {
            return m_error;
        }

        abstract protected bool Next();

        //protected void next_step (Msg msg_, int state_, bool beginning_) {
        //    if (msg_ == null)
        //        next_step((ByteBuffer) null, 0, state_, beginning_);
        //    else
        //        next_step(msg_.data(), msg_.size(), state_, beginning_);
        //}

        protected void NextStep(ByteArraySegment writePos, int toWrite,
                                 int state, bool beginning)
        {

            m_writePos = writePos;
            m_toWrite = toWrite;
            State = state;
            m_beginning = beginning;
        }

        //protected void next_step (byte[] buf_, int to_write_,
        //        int next_, bool beginning_)
        //{
        //    write_buf = null;
        //    write_array = buf_;
        //    write_pos = 0;
        //    to_write = to_write_;
        //    next = next_;
        //    beginning = beginning_;
        //}

    }
}
