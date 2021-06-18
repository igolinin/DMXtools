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
        [SerializeField]
        public int dmxAddress;
        protected int numberOfChannels;
        [SerializeField]
        protected int universe;
        [HideInInspector]
        public bool selected = false;
        [HideInInspector]
        public virtual bool isStandAlone {get; }
        
        
        [HideInInspector]
        public bool added = false;
        public virtual int getUniverse { get; }
        public virtual int getDmxAddress { get { return dmxAddress; } set { SetAddress(value); } }
        public virtual Dictionary<string, int> getChannelFunctions { get; }
        public virtual int getNumberOfChannels { get{return getChannelFunctions.Count;} }
        public virtual void OnEnable()
        {
            FindDataMap();
          
            if (PrefabUtility.IsPartOfPrefabInstance(transform.gameObject))
            {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(transform.gameObject);
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            
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
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawGameObjectName(DMXFixture fixture, GizmoType gizmoType)
        {
            Handles.Label(fixture.transform.position + new Vector3(0, 1.5f, -0.5f), fixture.universe.ToString()+"-"+fixture.dmxAddress.ToString());
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