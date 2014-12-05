﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using AsyncIO;

namespace NetMQ.zmq.Transports.PGM
{
    class PgmSender : IOObject, IEngine, IProcatorEvents
    {
        private readonly Options m_options;
        private readonly Address m_addr;
        private V1Encoder m_encoder;

        private AsyncSocket m_socket;
        private PgmSocket m_pgmSocket;

        private ByteArraySegment m_outBuffer;
        private int m_outBufferSize;

        private int m_writeSize;

        enum State
        {
            Idle,
            Connecting,
            Active,
            ActiveSendingIdle,
            Error
        }

        private State m_state;
        private PgmAddress m_pgmAddress;

        public PgmSender(IOThread ioThread, Options options, Address addr)
            : base(ioThread)
        {
            m_options = options;
            m_addr = addr;
            m_encoder = null;
            m_outBuffer = null;
            m_outBufferSize = 0;
            m_writeSize = 0;
            m_encoder = new V1Encoder(0, m_options.Endian);

            m_state = State.Idle;
        }

        public void Init(PgmAddress pgmAddress)
        {
            m_pgmAddress = pgmAddress;

            m_pgmSocket = new PgmSocket(m_options, PgmSocketType.Publisher, m_addr.Resolved as PgmAddress);
            m_pgmSocket.Init();

            m_socket = m_pgmSocket.Handle;

            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 0);

            m_socket.Bind(localEndpoint);

            m_pgmSocket.InitOptions();

            m_outBufferSize = Config.PgmMaxTPDU;
            m_outBuffer = new ByteArraySegment(new byte[m_outBufferSize]);            
        }

        public void Plug(IOThread ioThread, SessionBase session)
        {
            m_encoder.SetMsgSource(session);            
            
            // get the first message from the session because we don't want to send identities
            Msg msg = new Msg();
            msg.InitEmpty();

            bool ok = session.PullMsg(ref msg);

            if (ok)
            {
                msg.Close();
            }

            AddSocket(m_socket);

            m_state = State.Connecting;
            m_socket.Connect(m_pgmAddress.Address);            
        }

        public void Terminate()
        {
            RemoveSocket(m_socket);
            m_encoder.SetMsgSource(null);
        }


        public void ActivateOut()
        {
            if (m_state == State.ActiveSendingIdle)
            {
                m_state = State.Active;
                m_writeSize = 0;
                BeginSending();
            }            
        }

        public void ActivateIn()
        {
            Debug.Assert(false);
        }

        public override void OutCompleted(SocketError socketError, int bytesTransferred)
        {
            if (m_state == State.Connecting)
            {
                if (socketError == SocketError.Success)
                {
                    m_state = State.Active;
                    m_writeSize = 0;

                    BeginSending();
                }
                else
                {
                    m_state = State.Error;
                    NetMQException.Create(socketError);
                }
            }
            else if (m_state == State.Active)
            {
                //  We can write either all data or 0 which means rate limit reached.
                if (socketError == SocketError.Success && bytesTransferred == m_writeSize)
                {
                    m_writeSize = 0;

                    BeginSending();
                }
                else
                {
                    Debug.Assert(false);

                    throw NetMQException.Create(ErrorHelper.SocketErrorToErrorCode(socketError));
                }
            }
            else
            {
                Debug.Assert(false);
            }           
        }

        private void BeginSending()
        {
            //  If write buffer is empty,  try to read new data from the encoder.
            if (m_writeSize == 0)
            {
                //  First two bytes (sizeof uint16_t) are used to store message 
                //  offset in following steps. Note that by passing our buffer to
                //  the get data function we prevent it from returning its own buffer.
                ByteArraySegment bf = new ByteArraySegment(m_outBuffer, sizeof(ushort));
                int bfsz = m_outBufferSize - sizeof(ushort);
                int offset = -1;
                m_encoder.GetData(ref bf, ref bfsz, ref offset);

                //  If there are no data to write stop polling for output.
                if (bfsz == 0)
                {   
                    m_state = State.ActiveSendingIdle;
                    return;
                }

                //  Put offset information in the buffer.
                m_writeSize = bfsz + sizeof(ushort);

                m_outBuffer.PutUnsingedShort(m_options.Endian, offset == -1 ? (ushort)0xffff : (ushort)offset, 0);
            }

            try
            {
                m_socket.Send((byte[])m_outBuffer, m_outBuffer.Offset, m_writeSize, SocketFlags.None);           
            }
            catch (SocketException ex)
            {
                NetMQException.Create(ex.SocketErrorCode);
            }                      
        }

        public override void InCompleted(SocketError socketError, int bytesTransferred)
        {
            throw new NotImplementedException();
        }

        public override void TimerEvent(int id)
        {
            throw new NotImplementedException();
        }

    }
}
