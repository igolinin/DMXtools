using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IA;

namespace IA
{
    [ExecuteAlways]
    public abstract class DMXFixture : MonoBehaviour
    {


        [SerializeField]
        public ArtNetData artNetData;
        public bool selected;
        public virtual int getNumberOfChannels { get; }
         public virtual int getUniverse { get; }
        public virtual int getDmxAddress { get; }
        public virtual Dictionary<string, int> getChannelFunctions { get;}
        public virtual void OnEnable()
        {
            FindDataMap();
        }
        
        public void FindDataMap()
        {
            var path = "Assets/Scripts/ScriptableObjects/";
            var objectName = "DataMap.asset";
            artNetData = AssetDatabase.LoadAssetAtPath<ArtNetData>(path + objectName);
        }


        
    }
    public static class ChannelName
    {
        public const string RED = "Red";
        public const string GREEN = "Green";
        public const string BLUE = "Blue";
        public const string WHITE = "White";
        public const string PAN = "Pan";
        public const string TILT = "Tilt";
        public const string ZOOM = "Zoom";
        public const string STROBE = "Strobe";
    }
}