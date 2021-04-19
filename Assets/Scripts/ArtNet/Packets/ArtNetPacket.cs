using ArtNet.Enums;
using System.IO;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class ArtNetPacket
    {
        public ArtNetPacket(ArtNetOpCodes opCode)
        {
            OpCode = opCode;
        }

        public ArtNetPacket(ArtNetRecieveData data)
        {
            ArtNetBinaryReader packetReader = new ArtNetBinaryReader(new MemoryStream(data.buffer));
            ReadData(packetReader);
        }

        public byte[] ToArray()
        {
            MemoryStream stream = new MemoryStream();
            WriteData(new ArtNetBinaryWriter(stream));
            return stream.ToArray();
        }

        #region Packet Properties

        private string protocol = "Art-Net";

        public string Protocol
        {
            get { return protocol; }
            protected set
            {
                if (value.Length > 8)
                    protocol = value.Substring(0, 8);
                else
                    protocol = value;
            }
        }


        private short version = 14;

        public short Version
        {
            get { return version; }
            protected set { version = value; }
        }

        private ArtNetOpCodes opCode = ArtNetOpCodes.None;

        public virtual ArtNetOpCodes OpCode
        {
            get { return opCode; }
            protected set { opCode = value; }
        }

        #endregion

        public virtual void ReadData(ArtNetBinaryReader data)
        {
            Protocol = data.ReadNetworkString(8);
            OpCode = (ArtNetOpCodes)data.ReadNetwork16();

            //For some reason the poll packet header does not include the version.
            if (OpCode != ArtNetOpCodes.PollReply)
                Version = data.ReadNetwork16();

        }

        public virtual void WriteData(ArtNetBinaryWriter data)
        {
            data.WriteNetwork(Protocol, 8);
            data.WriteNetwork((short)OpCode);

            //For some reason the poll packet header does not include the version.
            if (OpCode != ArtNetOpCodes.PollReply)
                data.WriteNetwork(Version);

        }

        public static ArtNetPacket Create(ArtNetRecieveData data)
        {
            switch ((ArtNetOpCodes)data.OpCode)
            {
                case ArtNetOpCodes.Poll:
                    return new ArtPollPacket(data);
                case ArtNetOpCodes.PollReply:
                    return new ArtPollReplyPacket(data);
                case ArtNetOpCodes.Dmx:
                    return new ArtNetDmxPacket(data);
            }

            return null;

        }
    }
}
