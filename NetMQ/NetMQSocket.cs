﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NetMQ.zmq;
using NetMQ.zmq.Utils;

namespace NetMQ
{
    public abstract class NetMQSocket : IOutgoingSocket, IReceivingSocket,ISocketPollable, IDisposable
    {
        readonly SocketBase m_socketHandle;
        private bool m_isClosed = false;
        private NetMQSocketEventArgs m_socketEventArgs;

        private EventHandler<NetMQSocketEventArgs> m_receiveReady;

        private EventHandler<NetMQSocketEventArgs> m_sendReady;

        private Selector m_selector;

        protected NetMQSocket(SocketBase socketHandle)
        {
            m_selector = new Selector();
            m_socketHandle = socketHandle;
            Options = new SocketOptions(this);
            m_socketEventArgs = new NetMQSocketEventArgs(this);            
        }

        /// <summary>
        /// Occurs when at least one message may be received from the socket without blocking.
        /// </summary>
        public event EventHandler<NetMQSocketEventArgs> ReceiveReady
        {
            add
            {
                m_receiveReady += value;
                InvokeEventsChanged();
            }
            remove
            {
                m_receiveReady -= value;
                InvokeEventsChanged();
            }
        }

        /// <summary>
        /// Occurs when at least one message may be sent via the socket without blocking.
        /// </summary>
        public event EventHandler<NetMQSocketEventArgs> SendReady
        {
            add
            {
                m_sendReady += value;
                InvokeEventsChanged();
            }
            remove
            {
                m_sendReady -= value;
                InvokeEventsChanged();
            }
        }

        [Obsolete]
        public bool IgnoreErrors { get; set; }

        internal event EventHandler<NetMQSocketEventArgs> EventsChanged;

        internal int Errors { get; set; }


        private void InvokeEventsChanged()
        {
            var temp = EventsChanged;

            if (temp != null)
            {
                m_socketEventArgs.Init(PollEvents.None);
                temp(this, m_socketEventArgs);
            }
        }

        /// <summary>
        /// Set the options of the socket
        /// </summary>
        public SocketOptions Options { get; private set; }

        internal SocketBase SocketHandle
        {
            get
            {
                return m_socketHandle;
            }
        }

        NetMQSocket ISocketPollable.Socket
        {
            get { return this; }
        }

        /// <summary>
        /// Bind the socket to an address
        /// </summary>
        /// <param name="address">The address of the socket</param>
        public void Bind(string address)
        {
            m_socketHandle.CheckDisposed();

            m_socketHandle.Bind(address);            
        }

        /// <summary>
        /// Bind the socket to a random free port
        /// </summary>
        /// <param name="address">The address of the socket, omit the port</param>
        /// <returns>Chosen port number</returns>
        public int BindRandomPort(string address)
        {
            m_socketHandle.CheckDisposed();
            
            return m_socketHandle.BindRandomPort(address);
        }

        /// <summary>
        /// Connect the socket to an address
        /// </summary>
        /// <param name="address">Address to connect to</param>
        public void Connect(string address)
        {
            m_socketHandle.CheckDisposed();

            m_socketHandle.Connect(address);            
        }

        /// <summary>
        /// Disconnect the socket from specific address
        /// </summary>
        /// <param name="address">The address to disconnect from</param>
        public void Disconnect(string address)
        {
            m_socketHandle.CheckDisposed();

            m_socketHandle.TermEndpoint(address);
        }

        /// <summary>
        /// Unbind the socket from specific address
        /// </summary>
        /// <param name="address">The address to unbind from</param>
        public void Unbind(string address)
        {
            m_socketHandle.CheckDisposed();

            m_socketHandle.TermEndpoint(address);
        }

        /// <summary>
        /// Close the socket
        /// </summary>
        public void Close()
        {
            if (!m_isClosed)
            {
                m_isClosed = true;

                m_socketHandle.CheckDisposed();
                m_socketHandle.Close();                
            }
        }

        /// <summary> Wait until message is ready to be received from the socket. </summary>
        public void Poll()
        {
            Poll(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Wait until message is ready to be received from the socket or until timeout is reached
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Poll(TimeSpan timeout)
        {
            PollEvents events = GetPollEvents();

            var result = Poll(events, timeout);

            InvokeEvents(this, result);

            return result != PollEvents.None;
        }

        internal PollEvents Poll(PollEvents pollEvents, TimeSpan timeout)
        {
            SelectItem[] items = new[] {new SelectItem(SocketHandle, pollEvents),};

            m_selector.Select(items, 1, (int) timeout.TotalMilliseconds);
            return items[0].ResultEvent;
        }

        internal PollEvents GetPollEvents()
        {
            PollEvents events = PollEvents.PollError;

            if (m_sendReady != null)
            {
                events |= PollEvents.PollOut;
            }

            if (m_receiveReady != null)
            {
                events |= PollEvents.PollIn;
            }

            return events;
        }

        internal void InvokeEvents(object sender, PollEvents events)
        {
            if (!m_isClosed)
            {
                m_socketEventArgs.Init(events);

                if (events.HasFlag(PollEvents.PollIn))
                {
                    var temp = m_receiveReady;
                    if (temp != null)
                    {
                        temp(sender, m_socketEventArgs);
                    }
                }

                if (events.HasFlag(PollEvents.PollOut))
                {
                    var temp = m_sendReady;
                    if (temp != null)
                    {
                        temp(sender, m_socketEventArgs);
                    }
                }
            }
        }

        public virtual void Receive(ref Msg msg, SendReceiveOptions options)
        {                        
            m_socketHandle.Recv(ref msg, options);                        
        }       
                    
        public virtual void Send(ref Msg msg, SendReceiveOptions options)
        {
            m_socketHandle.Send(ref msg, options);
        }

        [Obsolete("Do not use this method if the socket is different from Subscriber and XSubscriber")]
        public virtual void Subscribe(string topic)
        {
            SetSocketOption(ZmqSocketOptions.Subscribe, topic);
        }

        [Obsolete("Do not use this method if the socket is different from Subscriber and XSubscriber")]
        public virtual void Subscribe(byte[] topic)
        {
            SetSocketOption(ZmqSocketOptions.Subscribe, topic);
        }

        [Obsolete("Do not use this method if the socket is different from Subscriber and XSubscriber")]
        public virtual void Unsubscribe(string topic)
        {
            SetSocketOption(ZmqSocketOptions.Unsubscribe, topic);
        }

        [Obsolete("Do not use this method if the socket is different from Subscriber and XSubscriber")]
        public virtual void Unsubscribe(byte[] topic)
        {
            SetSocketOption(ZmqSocketOptions.Unsubscribe, topic);
        }

        public void Monitor(string endpoint, SocketEvent events = SocketEvent.All)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            if (endpoint == string.Empty)
            {
                throw new ArgumentException("Unable to publish socket events to an empty endpoint.", "endpoint");
            }

            m_socketHandle.CheckDisposed();

            m_socketHandle.Monitor(endpoint, events);
        }

        public bool HasIn
        {
            get
            {
                PollEvents pollEvents = GetSocketOptionX<PollEvents>(ZmqSocketOptions.Events);

                return pollEvents.HasFlag(PollEvents.PollIn);
            }
        }

        public bool HasOut
        {
            get
            {
                PollEvents pollEvents = GetSocketOptionX<PollEvents>(ZmqSocketOptions.Events);

                return pollEvents.HasFlag(PollEvents.PollOut);
            }
        }

        internal int GetSocketOption(ZmqSocketOptions socketOptions)
        {
            m_socketHandle.CheckDisposed();

            return m_socketHandle.GetSocketOption(socketOptions);
        }

        internal T GetSocketOptionX<T>(ZmqSocketOptions socketOptions)
        {
            m_socketHandle.CheckDisposed();

            return (T)m_socketHandle.GetSocketOptionX(socketOptions);
        }

        internal TimeSpan GetSocketOptionTimeSpan(ZmqSocketOptions socketOptions)
        {
            return TimeSpan.FromMilliseconds(GetSocketOption(socketOptions));
        }

        internal long GetSocketOptionLong(ZmqSocketOptions socketOptions)
        {
            return GetSocketOptionX<long>(socketOptions);
        }
     
        internal void SetSocketOption(ZmqSocketOptions socketOptions, int value)
        {
            m_socketHandle.CheckDisposed();

            m_socketHandle.SetSocketOption(socketOptions, value);
        }

        internal void SetSocketOptionTimeSpan(ZmqSocketOptions socketOptions, TimeSpan value)
        {
            SetSocketOption(socketOptions, (int)value.TotalMilliseconds);
        }

        internal void SetSocketOption(ZmqSocketOptions socketOptions, object value)
        {
            m_socketHandle.CheckDisposed();

            m_socketHandle.SetSocketOption(socketOptions, value);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
