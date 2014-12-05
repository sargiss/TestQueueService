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
using System.Diagnostics;

namespace NetMQ.zmq.Patterns
{
    class Rep : Router
    {
        public class RepSession : Router.RouterSession
        {
            public RepSession(IOThread ioThread, bool connect,
                              SocketBase socket, Options options,
                              Address addr)
                : base(ioThread, connect, socket, options, addr)
            {
            }
        }
        //  If true, we are in process of sending the reply. If false we are
        //  in process of receiving a request.
        private bool m_sendingReply;

        //  If true, we are starting to receive a request. The beginning
        //  of the request is the backtrace stack.
        private bool m_requestBegins;


        public Rep(Ctx parent, int threadId, int socketId)
            : base(parent, threadId, socketId)
        {

            m_sendingReply = false;
            m_requestBegins = true;

            m_options.SocketType = ZmqSocketType.Rep;
        }

        protected override bool XSend(ref Msg msg, SendReceiveOptions flags)
        {
            //  If we are in the middle of receiving a request, we cannot send reply.
            if (!m_sendingReply)
            {
                throw new FiniteStateMachineException("Cannot send another reply");        
            }

            bool more = msg.HasMore;

            //  Push message to the reply pipe.
            bool isMessageSent = base.XSend(ref msg, flags);

            if (!isMessageSent)
            {
                return false;
            }
            //  If the reply is complete flip the FSM back to request receiving state.
            else if (!more)
                m_sendingReply = false;

            return true;
        }

        protected override bool XRecv(SendReceiveOptions flags, ref Msg msg)
        {
            bool isMessageAvailable;

            //  If we are in middle of sending a reply, we cannot receive next request.
            if (m_sendingReply)
            {
                throw new FiniteStateMachineException("Cannot receive another request");                 
            }

            //  First thing to do when receiving a request is to copy all the labels
            //  to the reply pipe.
            if (m_requestBegins)
            {
                while (true)
                {
                    isMessageAvailable = base.XRecv(flags, ref msg);

                    if (!isMessageAvailable)
                    {
                        return false;
                    }                  

                    if (msg.HasMore)
                    {
                        //  Empty message part delimits the traceback stack.
                        bool bottom = (msg.Size == 0);

                        //  Push it to the reply pipe.
                        isMessageAvailable = base.XSend(ref msg, flags);
                        if (!isMessageAvailable)
                        {
                            return false;
                        }

                        if (bottom)
                            break;
                    }
                    else
                    {
                        //  If the traceback stack is malformed, discard anything
                        //  already sent to pipe (we're at end of invalid message).
                        base.Rollback();
                    }
                }
                m_requestBegins = false;
            }

            //  Get next message part to return to the user.
            isMessageAvailable = base.XRecv(flags, ref msg);

            if (!isMessageAvailable)
            {
                return false;
            }          

            //  If whole request is read, flip the FSM to reply-sending state.
            if (!msg.HasMore)
            {
                m_sendingReply = true;
                m_requestBegins = true;
            }

            return true;
        }

        protected override bool XHasIn()
        {
            if (m_sendingReply)
                return false;

            return base.XHasIn();
        }

        protected override bool XHasOut()
        {
            if (!m_sendingReply)
                return false;

            return base.XHasOut();
        }
    }
}
