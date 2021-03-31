using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA
{
    [ExecuteAlways]
    public class MovingLight : DMXFixture
    {
        public override int getUniverse { get { return universe; } }
        public override int getDmxAddress { get { return dmxAddress; } }
        public override int getNumberOfChannels { get { return numberOfChannels; } }
        public override Dictionary<string, int> getChannelFunctions { get { return channelFunctions; } }

       
        [SerializeField]
        Transform panMotor;
    
        new Light light;

        [Header("rotateProps")]
        public float panMovement = 360f;
        public float tiltMovement = 180f;
        float panTarget;
        float tiltTarget;
        float minRotSpeed = 1f;
        float maxRotSpeed = 100f;
        private Dictionary<string, int> channelFunctions = new Dictionary<string, int> { { ChannelName.RED, 0 }, { ChannelName.GREEN, 1 }, { ChannelName.BLUE, 2 }, { ChannelName.WHITE, 3 },{ ChannelName.TILT, 4 }, { ChannelName.PAN, 5 }  };

        float pan;
        float tilt;
        float rotSpeed;

        
        void SetPan()
        {
            pan = panMotor.localEulerAngles.y;
            panTarget = (artNetData.dmxDataMap[universe-1][dmxAddress + (int)channelFunctions[ChannelName.PAN]] ) * panMovement / 256f;
        }
        void SetTilt()
        {
            tiltTarget = (artNetData.dmxDataMap[universe-1][dmxAddress + (int)channelFunctions[ChannelName.TILT]]  ) * tiltMovement / 256f;
            tilt = light.transform.localEulerAngles.x;
        }

        void Update()
        {
             GetWireData();
             UpdateRotation();
        }
        void UpdateRotation()
        {
            
            var dpan = (panTarget - pan);
            var dtilt = (tiltTarget - tilt);
            
            if (0 != dpan)
            {
                panMotor.transform.localEulerAngles = new Vector3(0,panTarget,0) ;
            }
            if (0 != dtilt)
            {
                light.transform.localEulerAngles = new Vector3(tiltTarget,0,0) ;
            }
        }
        void UpdateStrobo()
        {

        }
        void SetColor()
        {
            var color = light.color;

            color.r = artNetData.dmxDataMap[universe-1][dmxAddress + (int)channelFunctions[ChannelName.RED]] / 256f;
            color.g = artNetData.dmxDataMap[universe-1][dmxAddress + (int)channelFunctions[ChannelName.GREEN]] / 256f;
            color.b = artNetData.dmxDataMap[universe-1][dmxAddress + (int)channelFunctions[ChannelName.BLUE]] / 256f;
            color += Color.white * 0.5f * artNetData.dmxDataMap[universe-1][dmxAddress + (int)channelFunctions[ChannelName.WHITE]] / 256f;

            light.color = color;
        }
        
        public override void OnEnable()
        {

            light = GetComponent<Light>();
            FindDataMap();
            artNetData.dmxUpdate.AddListener(UpdateDMX);
        }
        void UpdateDMX()
        {
            GetWireData();
            UpdateRotation();
        }
        void GetWireData()
        {
            SetColor();
            SetPan();
            SetTilt();
        }
    }
}
