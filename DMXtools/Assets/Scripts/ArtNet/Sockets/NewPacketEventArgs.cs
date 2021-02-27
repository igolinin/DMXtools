using System;
using System.Net;

namespace ArtNet.Sockets
{
    public class NewPacketEventArgs<TPacketType> : EventArgs
    {
        public NewPacketEventArgs(IPEndPoint source, TPacketType packet)
        {
            Source = source;
            Packet = packet;
        }

        private IPEndPoint source;

        public IPEndPoint Source
        {
            get { return source; }
            protected set { source = value; }
        }

        private TPacketType packet;

        public TPacketType Packet
        {
            get { return packet; }
            private set { packet = value; }
        }

    }
}
