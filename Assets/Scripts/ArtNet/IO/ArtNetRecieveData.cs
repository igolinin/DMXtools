namespace ArtNet.IO
{
    public class ArtNetRecieveData
    {
        public byte[] buffer = new byte[1500];
        public int bufferSize = 1500;
        public int DataLength = 0;

        public bool Valid
        {
            get { return DataLength > 12; }
        }

        public int OpCode
        {
            get
            {
                return buffer[9] + (buffer[8] << 8);
            }
        }

    }
}
