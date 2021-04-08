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
        protected ArtNetData artNetData;
        [SerializeField]
        public int dmxAddress;

        [SerializeField]
        protected int numberOfChannels;
        [SerializeField]
        protected int universe;
        public bool selected=false;
        public bool added = false;
        public virtual int getNumberOfChannels { get; }
        public virtual int getUniverse { get; }
        public virtual int getDmxAddress { get{return dmxAddress;} set{SetAddress(value);}}
        public virtual Dictionary<string, int> getChannelFunctions { get;}
        public virtual void OnEnable()
        {
            FindDataMap();
        }
        public void SetAddress(int value)
        {
            getDmxAddress = value;
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
        public const string YELLOW = "Yellow";
        public const string HARD_SOFT = "Hard / Soft";
        public const string SILENT_NORMAL_BOOST = "Silent / Normal / Boost";
        public const string DIMMER = "Dimmer";
    }
}