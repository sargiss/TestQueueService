/*
    Copyright (c) 2007-2012 iMatix Corporation
    Copyright (c) 2009-2011 250bpm s.r.o.
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
using System.Text;

namespace NetMQ.zmq.Patterns
{
    class Sub : XSub
    {
        public class SubSession : XSub.XSubSession
        {

            public SubSession(IOThread ioThread, bool connect,
                              SocketBase socket, Options options, Address addr)
                : base(ioThread, connect, socket, options, addr)
            {

            }

        }

        public Sub(Ctx parent, int threadId, int socketId)
            : base(parent, threadId, socketId)
        {
            m_options.SocketType = ZmqSocketType.Sub;

            //  Switch filtering messages on (as opposed to XSUB which where the
            //  filtering is off).
            m_options.Filter = true;
        }

        protected override bool XSetSocketOption(ZmqSocketOptions option, Object optval)
        {
            if (option != ZmqSocketOptions.Subscribe && option != ZmqSocketOptions.Unsubscribe)
            {
                return false;
            }

            byte[] val;

            if (optval is String)
                val = Encoding.ASCII.GetBytes((String)optval);
            else if (optval is byte[])
                val = (byte[])optval;
            else
                throw new InvalidException();

            //  Create the subscription message.
            Msg msg = new Msg();
            msg.InitPool(val.Length + 1);
            if (option == ZmqSocketOptions.Subscribe)
                msg.Put((byte)1);
            else if (option == ZmqSocketOptions.Unsubscribe)
                msg.Put((byte)0);
            msg.Put(val, 1, val.Length);

            try
            {
                //  Pass it further on in the stack.
                bool isMessageSent = base.XSend(ref msg, 0);

                if (!isMessageSent)
                {
                    throw new AgainException();
                }
            }
            finally
            {
                msg.Close();
            }

            return true;
        }

        protected override bool XSend(ref Msg msg, SendReceiveOptions flags)
        {
            //  Overload the XSUB's send.
            throw new NotSupportedException("Send not supported on sub socket");
        }

        protected override bool XHasOut()
        {
            //  Overload the XSUB's send.
            return false;
        }
    }
}
