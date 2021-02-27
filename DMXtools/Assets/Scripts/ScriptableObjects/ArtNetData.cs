using System.Collections;
using System.Collections.Generic;
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
            if(dmxDataMap == null)
            {
                Debug.Log("not initialized");
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
    }
}

