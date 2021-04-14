
using System.Collections.Generic;
using UnityEngine;


namespace IA
{

    [ExecuteAlways]
    public class MixPanel60v1 : DMXFixture
    {
        public override int getUniverse { get { return universe = 8; } }
        public override int getDmxAddress { get { return dmxAddress; } }
        public override int getNumberOfChannels { get { return numberOfChannels; } }
        public override Dictionary<string, int> getChannelFunctions { get { return channelFunctions; } }
        [SerializeField]
        bool sendDmx;
        [SerializeField]
        RenderTexture colorProbeTexture;
        [SerializeField]
        Color32 color;
        [SerializeField]
        string[] Channels;
        [SerializeField]
        private Dictionary<string, int> channelFunctions = new Dictionary<string, int> {{ ChannelName.RED, 0 }, { ChannelName.GREEN, 1 }, { ChannelName.BLUE, 2 }, { ChannelName.WHITE, 3 },{ ChannelName.YELLOW, 4 }, { ChannelName.HARD_SOFT, 5 }, { ChannelName.SILENT_NORMAL_BOOST, 6 }  };
        void Update()
        {
            if(sendDmx)
            {
                ReadColor();
                UpdateDMX();
            }
            
        }
        void ReadColor()
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(colorProbeTexture.width, colorProbeTexture.height, 24);
            Graphics.Blit(colorProbeTexture, tempTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tempTexture;

            Texture2D tempTexture2D = new Texture2D(colorProbeTexture.width, colorProbeTexture.height);
            tempTexture2D.ReadPixels(new Rect(0,0, tempTexture.width, tempTexture.height), 0,0);
            tempTexture2D.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tempTexture);

            Color32[] colors = tempTexture2D.GetPixels32();
            color = colors[0];
            
            //Debug.Log(colors[0].r+"  "+colors[0].g+"  "+colors[0].b+"  "+colors.Length);
        }
        void UpdateDMX()
        {
            artNetData.dmxDataMap[getUniverse - 1][getDmxAddress - 1 + getChannelFunctions[ChannelName.RED]] = (byte)color.r;
            artNetData.dmxDataMap[getUniverse - 1][getDmxAddress - 1 + getChannelFunctions[ChannelName.GREEN]] = (byte)color.g;
            artNetData.dmxDataMap[getUniverse - 1][getDmxAddress - 1 + getChannelFunctions[ChannelName.BLUE]] = (byte)color.b;
        }

    }
    
}
