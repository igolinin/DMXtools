
using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    [System.Serializable]
    public class ArtNetDmxPacket : ArtNetPacket
    {
        public ArtNetDmxPacket()
            : base(ArtNetOpCodes.Dmx)
        {
        }

        public ArtNetDmxPacket(ArtNetRecieveData data)
            : base(data)
        {

        }

        #region Packet Properties
        
        private byte sequence = 0;

        public byte Sequence
        {
            get { return sequence; }
            set { sequence = value; }
        }
        
        private byte physical = 0;

        public byte Physical
        {
            get { return physical; }
            set { physical = value; }
        }

        [UnityEngine.SerializeField]
        private short universe = 0;

        public short Universe
        {
            get { return universe; }
            set { universe = value; }
        }

        public short Length
        {
            get
            {
                if (dmxData == null)
                    return 0;
                return (short)dmxData.Length;
            }
        }

        [UnityEngine.SerializeField]
        private byte[] dmxData = null;

        public byte[] DmxData
        {
            get { return dmxData; }
            set { dmxData = value; }
        }

        #endregion

        public override void ReadData(ArtNetBinaryReader data)
        {
            base.ReadData(data);

            Sequence = data.ReadByte();
            Physical = data.ReadByte();
            Universe = data.ReadInt16();
            int length = data.ReadNetwork16();
            DmxData = data.ReadBytes(length);
        }

        public override void WriteData(ArtNetBinaryWriter data)
        {
            base.WriteData(data);

            data.Write(Sequence);
            data.Write(Physical);
            data.Write(Universe);
            data.WriteNetwork(Length);
            data.Write(DmxData);
        }

    }
}
