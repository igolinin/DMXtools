namespace ArtNet.Enums
{
    public enum ArtNetOpCodes
    {
        None = 0,
        Poll = 0x20,
        PollReply = 0x21,
        Dmx = 0x50,
        TodRequest = 0x80,
        TodData = 0x81,
        TodControl = 0x82,
        Rdm = 0x83,
        RdmSub = 0x84,
    }

    public enum ArtNetStyles
    {
        StNode = 0x00,
        StServer = 0x01,
        StMedia = 0x02,
        StRoute = 0x03,
        StBackup = 0x04,
        StConfig = 0x05
    }

}
