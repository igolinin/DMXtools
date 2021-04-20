
using System.Collections.Generic;
using UnityEngine;


namespace IA
{

    [ExecuteAlways]
    public class GenericDMXFixture : DMXFixture
    {
        public override int getUniverse { get { return universe = 8; } }
        public override int getDmxAddress { get { return dmxAddress; } }
        public override int getNumberOfChannels { get { return numberOfChannels; } }
        public override Dictionary<string, int> getChannelFunctions { get { return channelFunctions; } }
        
        [SerializeField]
        bool sendDmx;
        [SerializeField]
        [Range(-255, 255)]
        int correctionRed;
        [SerializeField]
        [Range(-255, 255)]
        int correctionGreen;
        [SerializeField]
        [Range(-255, 255)]
        int correctionBlue;
        [SerializeField]
        RenderTexture colorProbeTexture;
        [SerializeField]
        Color32 color;
        [SerializeField]
        string[]  channels;
        [SerializeField]
        //private Dictionary<string, int> channelFunctions = new Dictionary<string, int> {{ ChannelName.RED, 0 }, { ChannelName.GREEN, 1 }, { ChannelName.BLUE, 2 }, { ChannelName.WHITE, 3 },{ ChannelName.YELLOW, 4 }, { ChannelName.HARD_SOFT, 5 }, { ChannelName.SILENT_NORMAL_BOOST, 6 }  };
        private Dictionary<string, int> channelFunctions;
        public override void OnEnable()
        {
            base.OnEnable();
            InitFunctions();
            
        }
        void Update()
        {
            if(sendDmx && colorProbeTexture != null)
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
            artNetData.dmxDataMap[getUniverse - 1][getDmxAddress - 1 + getChannelFunctions[ChannelName.RED]] = correctColor(color.r, correctionRed);
            artNetData.dmxDataMap[getUniverse - 1][getDmxAddress - 1 + getChannelFunctions[ChannelName.GREEN]] =correctColor(color.g, correctionGreen);
            artNetData.dmxDataMap[getUniverse - 1][getDmxAddress - 1 + getChannelFunctions[ChannelName.BLUE]] = correctColor(color.b, correctionBlue);
        }
        byte correctColor(byte component, int bias)
        {
            return (byte)Mathf.Clamp(component+bias, 0f, 255f);
        }
        void InitFunctions()
        {
            channelFunctions = new Dictionary<string, int>();
            for(int i = 0; i < channels.Length; i++)
            {
                channelFunctions.Add(channels[i], i);
            }
            numberOfChannels = channels.Length;
        }
        

    }
    
}

