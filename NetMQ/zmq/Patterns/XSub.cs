/*
    Copyright (c) 2010-2011 250bpm s.r.o.
    Copyright (c) 2011 VMware, Inc.
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

using System.Configuration;
using System.Diagnostics;
using NetMQ.zmq.Patterns.Utils;

namespace NetMQ.zmq.Patterns
{
    class XSub : SocketBase
    {
        public class XSubSession : SessionBase
        {

            public XSubSession(IOThread ioThread, bool connect,
                               SocketBase socket, Options options, Address addr) :
                base(ioThread, connect, socket, options, addr)
            {
            }

        }

        //  Fair queueing object for inbound pipes.
        private readonly FairQueueing m_fairQueueing;

        //  Object for distributing the subscriptions upstream.
        private readonly Distribution m_distribution;

        //  The repository of subscriptions.
        private readonly Trie m_subscriptions;

        //  If true, 'message' contains a matching message to return on the
        //  next recv call.
        private bool m_hasMessage;
        private Msg m_message;

        //  If true, part of a multipart message was already received, but
        //  there are following parts still waiting.
        private bool m_more;
        private static readonly Trie.TrieDelegate s_sendSubscription;

        static XSub()
        {
            s_sendSubscription = (data, size, arg) =>
            {
                Pipe pipe = (Pipe)arg;

                //  Create the subsctription message.
                Msg msg = new Msg();
                msg.InitPool(size + 1);
                msg.Put((byte)1);
                msg.Put(data, 1, size);

                //  Send it to the pipe.
                bool sent = pipe.Write(ref msg);
                //  If we reached the SNDHWM, and thus cannot send the subscription, drop
                //  the subscription message instead. This matches the behaviour of
                //  zmq_setsockopt(ZMQ_SUBSCRIBE, ...), which also drops subscriptions
                //  when the SNDHWM is reached.
                if (!sent)
                    msg.Close();
            };
        }

        public XSub(Ctx parent, int threadId, int socketId)
            : base(parent, threadId, socketId)
        {

            m_options.SocketType = ZmqSocketType.Xsub;
            m_hasMessage = false;
            m_more = false;

            m_options.Linger = 0;
            m_fairQueueing = new FairQueueing();
            m_distribution = new Distribution();
            m_subscriptions = new Trie();

            m_message = new Msg();
            m_message.InitEmpty();
        }

        public override void Destroy()
        {
            base.Destroy();
            m_message.Close();
        }

        protected override void XAttachPipe(Pipe pipe, bool icanhasall)
        {
            Debug.Assert(pipe != null);
            m_fairQueueing.Attach(pipe);
            m_distribution.Attach(pipe);

            //  Send all the cached subscriptions to the new upstream peer.
            m_subscriptions.Apply(s_sendSubscription, pipe);
            pipe.Flush();
        }

        protected override void XReadActivated(Pipe pipe)
        {
            m_fairQueueing.Activated(pipe);
        }

        protected override void XWriteActivated(Pipe pipe)
        {
            m_distribution.Activated(pipe);
        }

        protected override void XTerminated(Pipe pipe)
        {
            m_fairQueueing.Terminated(pipe);
            m_distribution.Terminated(pipe);
        }

        protected override void XHiccuped(Pipe pipe)
        {
            //  Send all the cached subscriptions to the hiccuped pipe.
            m_subscriptions.Apply(s_sendSubscription, pipe);
            pipe.Flush();
        }

        protected override bool XSend(ref Msg msg, SendReceiveOptions flags)
        {
            byte[] data = msg.Data;
            int size = msg.Size;
            
            if (size > 0 && data[0] == 1)
            {
                // Process the subscription.
                if (m_subscriptions.Add(data, 1, size-1))
                {
                    m_distribution.SendToAll(ref msg, flags);
                    return true;
                }               
            }
            else if (size > 0 && data[0] == 0)
            {
                if (m_subscriptions.Remove(data, 1,size-1))
                {
                    m_distribution.SendToAll(ref msg, flags);
                    return true;
                }
            }
            else
            {
                // upstream message unrelated to sub/unsub
                m_distribution.SendToAll(ref msg, flags);

                return true;
            }

            msg.Close();
            msg.InitEmpty();

            return true;
        }

        protected override bool XHasOut()
        {
            //  Subscription can be added/removed anytime.
            return true;
        }

        protected override bool XRecv(SendReceiveOptions flags, ref Msg msg)
        {
            //  If there's already a message prepared by a previous call to zmq_poll,
            //  return it straight ahead.

            if (m_hasMessage)
            {
                msg.Move(ref m_message);
                m_hasMessage = false;
                m_more = msg.HasMore;
                return true;
            }

            //  TODO: This can result in infinite loop in the case of continuous
            //  stream of non-matching messages which breaks the non-blocking recv
            //  semantics.
            while (true)
            {

                //  Get a message using fair queueing algorithm.
                bool isMessageAvailable = m_fairQueueing.Recv(ref msg);

                //  If there's no message available, return immediately.
                //  The same when error occurs.
                if (!isMessageAvailable)
                {
                    return false;
                }

                //  Check whether the message matches at least one subscription.
                //  Non-initial parts of the message are passed 
                if (m_more || !m_options.Filter || Match(msg))
                {
                    m_more = msg.HasMore;
                    return true;
                }

                //  Message doesn't match. Pop any remaining parts of the message
                //  from the pipe.
                while (msg.HasMore)
                {
                    isMessageAvailable = m_fairQueueing.Recv(ref msg);

                    Debug.Assert(isMessageAvailable);
                }
            }
        }

        protected override bool XHasIn()
        {
            //  There are subsequent parts of the partly-read message available.
            if (m_more)
                return true;

            //  If there's already a message prepared by a previous call to zmq_poll,
            //  return straight ahead.
            if (m_hasMessage)
                return true;

            //  TODO: This can result in infinite loop in the case of continuous
            //  stream of non-matching messages.
            while (true)
            {

                //  Get a message using fair queueing algorithm.
                bool isMessageAvailable = m_fairQueueing.Recv(ref m_message);

                //  If there's no message available, return immediately.
                //  The same when error occurs.
                if (!isMessageAvailable)
                {
                    return false;
                }

                //  Check whether the message matches at least one subscription.
                if (!m_options.Filter || Match(m_message))
                {
                    m_hasMessage = true;
                    return true;
                }

                //  Message doesn't match. Pop any remaining parts of the message
                //  from the pipe.
                while (m_message.HasMore)
                {
                    isMessageAvailable = m_fairQueueing.Recv(ref m_message);

                    Debug.Assert(isMessageAvailable);
                }
            }
        }

        private bool Match(Msg msg)
        {
            return m_subscriptions.Check(msg.Data, msg.Size);
        }
    }
}
