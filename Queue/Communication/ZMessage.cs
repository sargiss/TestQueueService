using System.Collections.Generic;
using System.Text;
using NetMQ;
using Queue.Common;

namespace Queue
{
    public class ZMessage
    {
        readonly protected Encoding encoding = Encoding.Unicode;
        readonly protected List<byte[]> frames = new List<byte[]>();

        public ZMessage()
        {
        }

        public ZMessage(NetMQSocket NetMQSocket)
        {
            Receive(NetMQSocket);
        }

        public ZMessage(object body):this(body.ToJson())
        { }

        public ZMessage(string body, Encoding encoding)
        {
            frames.Add(encoding.GetBytes(body));
            this.encoding = encoding;
        }

        public ZMessage(string body)
        {
            frames.Add(encoding.GetBytes(body));
        }

        public ZMessage(byte[] body)
        {
            frames.Add(body);
        }

        public void Receive(NetMQSocket NetMQSocket)
        {
            var zmqMessage = NetMQSocket.ReceiveMessage();

            foreach (var frame in zmqMessage)
            {
                var s = Encoding.Unicode.GetString(frame.Buffer);
                frames.Insert(0, frame.Buffer);
            }

            return;
            bool hasMore = false;
           
            do
            {
                var f = NetMQSocket.Receive(false, out hasMore);
                frames.Insert(0, f);
            } while (hasMore);
            
        }

        public void Send(NetMQSocket socket)
        {
            for (int index = frames.Count - 1; index > 0; index--)
            {
                socket.SendMore(frames[index]);
            }

            socket.Send(frames[0]);
        }

        public string BodyToString()
        {
            return encoding.GetString(Body);
        }

        public void StringToBody(string body)
        {
            Body = encoding.GetBytes(body);
        }

        public void Append(byte[] data)
        {
            frames.Insert(0, data);
        }

        public byte[] Pop()
        {
            byte[] data = frames[frames.Count - 1];
            frames.RemoveAt(frames.Count - 1);
            return data;
        }

        public void Push(byte[] data)
        {
            frames.Add(data);
        }

        public void Wrap(byte[] address, byte[] delim)
        {
            if (delim != null)
            {
                frames.Add(delim);
            }
            frames.Add(address);
        }

        public byte[] Unwrap()
        {
            byte[] addr = Pop();
            if (Address.Length == 0)
            {
                Pop();
            }
            return addr;
        }

        public int FrameCount
        {
            get { return frames.Count; }
        }

        public byte[] Address
        {
            get { return frames[frames.Count - 1]; }
            set { frames.Add(value); }
        }

        public byte[] Body
        {
            get { return frames[0]; }
            set { frames[0] = value; }
        }
    }
}