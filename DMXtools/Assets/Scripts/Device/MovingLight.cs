using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA
{
    public class MovingLight : DMXFixture
    {
         private int dmxAddress;
        public override int getDmxAddress { get { return dmxAddress; } }

        [SerializeField]
        int numberOfChannels;
        public override int getNumberOfChannels { get { return numberOfChannels; } }
        public Transform panRotater;
        public Transform tiltRotater;
        new public Light light;

        [Header("rotateProps")]
        public float panMovement = 360f;
        public float tiltMovement = 270f;
        public float panTarget;
        public float tiltTarget;
        public float minRotSpeed = 1f;
        public float maxRotSpeed = 100f;

        public float pan;
        public float tilt;
        public float rotSpeed;

        /* public override void SetData(byte[] dmxData)
        {
            //base.SetData(dmxData);

        } */
        void SetPan(byte panData, byte panFineData = 0)
        {
            panTarget = (panData - 127 + panFineData / 256f) * panMovement / 256f;
        }
        void SetTilt(byte tiltData, byte tiltFineData = 0)
        {
            tiltTarget = (tiltData - 127f + tiltFineData / 256f) * tiltMovement / 256f;
        }

        private void Update()
        {
            UpdateRotation();
        }
        void UpdateRotation()
        {
            var dpan = (panTarget - pan);
            var dtilt = (tiltTarget - tilt);
            if (0 < dpan)
            {
                dpan = Mathf.Min(Mathf.Abs(dpan), Time.deltaTime * rotSpeed) * Mathf.Sign(dpan);
                pan += dpan;
                panRotater.Rotate(0, 0, dpan);
            }
            if (0 < dtilt)
            {
                dtilt = Mathf.Min(Mathf.Abs(dtilt), Time.deltaTime * rotSpeed) * Mathf.Sign(dtilt);
                tilt += dtilt;
                tiltRotater.Rotate(dtilt, 0, 0);
            }
        }
        void UpdateStrobo()
        {

        }
        void SetColor()
        {

        }
        float GetColor(byte val, byte fineVal = 0)
        {
            return val / 256f + fineVal / (256f * 256f);
        }
    }
}
