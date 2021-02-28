
using System.Collections.Generic;
using UnityEngine;


namespace IA
{

    [ExecuteAlways]
    public class MixPanel60v1 : DMXFixture
    {
        public override int getUniverse { get { return universe = 7; } }
        public override int getDmxAddress { get { return dmxAddress; } }
        public override int getNumberOfChannels { get { return numberOfChannels; } }
        public override Dictionary<string, int> getChannelFunctions { get { return channelFunctions; } }
        [SerializeField]
        string[] Channels;
        [SerializeField]
        private Dictionary<string, int> channelFunctions = new Dictionary<string, int> {{ ChannelName.RED, 0 }, { ChannelName.GREEN, 1 }, { ChannelName.BLUE, 2 }, { ChannelName.WHITE, 3 },{ ChannelName.YELLOW, 4 }, { ChannelName.HARD_SOFT, 5 }, { ChannelName.SILENT_NORMAL_BOOST, 6 }  };


    }
    
}
