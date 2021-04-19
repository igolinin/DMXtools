using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace IA
{
    [CreateAssetMenu(menuName = "ArtNet/DMX data map")]
    [System.Serializable]
    public class ArtNetData : ScriptableObject
    {
        public byte[][] dmxDataMap;
        public UnityEvent dmxUpdate;
        public void OnEnable()
        {
            dmxUpdate = new UnityEvent();
            if (dmxDataMap == null)
            {
                dmxDataMap = new byte[8][];
                for (int i = 0; i < 8; i++)
                    dmxDataMap[i] = new byte[512];
            }
        }
        public void SetData(int universe, byte[] dmxData)
        {
            dmxDataMap[universe] = dmxData;
            dmxUpdate.Invoke();
        }
        public void ResetData()
        {
            dmxDataMap = new byte[8][];
            for (int i = 0; i < 8; i++)
                dmxDataMap[i] = new byte[512];
            dmxUpdate.Invoke();
        }
        public void SetData(byte[][] dataMap)
        {
            //dmxDataMap = dataMap;
            for (int i = 0; i < 8; i++)
                Array.Copy(dataMap[i], dmxDataMap[i],  dataMap[i].Length);
            dmxUpdate.Invoke();
        }
        public byte[][] GetData()
        {
            return dmxDataMap;
            
        }
    }
}

