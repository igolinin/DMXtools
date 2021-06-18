using System.Collections.Generic;
using UnityEngine;


namespace IA
{
    [RequireComponent(typeof(Light))]
    [ExecuteAlways]
    public class RGBWLight : DMXFixture
    {
        public override int getUniverse { get { return universe; } }
        public override int getDmxAddress { get { return dmxAddress; } }
        //public override int getNumberOfChannels { get {return channelFunctions.Count;} }
        public override Dictionary<string, int> getChannelFunctions { get { return channelFunctions; } }


        private Dictionary<string, int> channelFunctions = new Dictionary<string, int> { { ChannelName.RED, 0 }, { ChannelName.GREEN, 1 }, { ChannelName.BLUE, 2 }, { ChannelName.WHITE, 3 } };
        new Light light;
        void GetWireData()
        {
            if (artNetData.dmxDataMap != null)
            {
                var color = light.color;

                color.r = artNetData.dmxDataMap[universe - 1][dmxAddress - 1 + (int)channelFunctions[ChannelName.RED]] / 256f;
                color.g = artNetData.dmxDataMap[universe - 1][dmxAddress - 1 + (int)channelFunctions[ChannelName.GREEN]] / 256f;
                color.b = artNetData.dmxDataMap[universe - 1][dmxAddress - 1 + (int)channelFunctions[ChannelName.BLUE]] / 256f;
                color += Color.white * 0.5f * artNetData.dmxDataMap[universe - 1][dmxAddress - 1 + (int)channelFunctions[ChannelName.WHITE]] / 256f;

                light.color = color;
            }
        }
        void Update()
        {
            GetWireData();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            light = GetComponent<Light>();
            FindDataMap();
            artNetData.dmxUpdate.AddListener(UpdateDMX);
        }
        void UpdateDMX()
        {
            GetWireData();
        }



    }
}
