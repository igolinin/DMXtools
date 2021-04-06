using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace IA
{
    [ExecuteAlways]
    public class CueEditor : EditorWindow
    {
        ArtNetData artNetData;
        CueStack cueStack;
        Rect header;
        Rect side;
        Rect body;
        Rect subHeader;
        string cueName;
        int currentCue;
        bool playback;

        [MenuItem("Window/ArtNet/Cue Editor")]
        static void Init()
        {
            CueEditor window = (CueEditor)EditorWindow.GetWindow(typeof(CueEditor));
            window.titleContent.text = "Cue Editor";
            window.Show();
        }
        void OnEnable()
        {
            FindDataMap();
            ReadData();
        }
        void OnDisable()
        {
            SaveData();
        }
        void OnGUI()
        {
            DrawLayouts();
            DrawHeader();
            DrawSidePanel();
            DrawBody();
        }
        void DrawLayouts()
        {
            header.x = 0;
            header.y = 0;
            header.width = Screen.width;
            header.height = 40;


            side.x = 0;
            side.y = header.height;
            side.width = 0;
            side.height = Screen.height - header.height;

            body.x = side.width;
            body.y = header.height;
            body.width = Screen.width - side.width;
            body.height = Screen.height - header.height - 20;

            subHeader.x = 0;
            subHeader.y = 0;
            subHeader.width = Screen.width;
            subHeader.height = 20;
        }

        void DrawHeader()
        {
            GUILayout.BeginArea(header);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save stack"))
            {
                //ClearOutput();
                SaveData();
            }
            if (GUILayout.Button("Clear Stack"))
            {
                cueStack.ClearStack();
            }
            playback = EditorGUILayout.Toggle("Playback", playback, GUILayout.Width(200));

            

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void Start()
        {
            FindDataMap();
        }
        void DrawSidePanel()
        {
            GUILayout.BeginArea(side);

            
            GUILayout.EndArea();
        }
        void DrawBody()
        {


            GUILayout.BeginArea(body);
            GUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            cueName = GUILayout.TextField(cueName);
            
            
            if (GUILayout.Button("Record Cue"))
            {
                RecordOutput(artNetData.dmxDataMap, cueName);
                cueName = "";
            }
            EditorGUILayout.EndHorizontal();
            if (cueStack != null)
            {
                string[] cueNames = cueStack.stack.Select(c => c.name).ToArray();
                int prevCue = currentCue;
                currentCue = GUILayout.SelectionGrid(currentCue, cueNames, 1);
                if (prevCue != currentCue && playback)
                {
                    artNetData.SetData(cueStack.stack[currentCue].cueData);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        private void FindDataMap()
        {
            var path = "Assets/Scripts/ScriptableObjects/";
            var objectName = "DataMap.asset";

            artNetData = AssetDatabase.LoadAssetAtPath<ArtNetData>(path + objectName);

            if (artNetData == null)
            {
                Debug.Log("Couldn't find dataMap. Creating dataMap...");
                artNetData = CreateInstance<ArtNetData>();
                AssetDatabase.CreateAsset(artNetData, path + objectName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        private void ClearOutput()
        {
            artNetData.ResetData();
        }
        void CallUpdate()
        {
            //Debug.Log("update is called");
            artNetData.dmxUpdate.Invoke();
        }
        private void RecordOutput(byte[][] dmxDataMap, string name)
        {
            if (cueStack != null)
            {
                var cue = new Cue(dmxDataMap, name);
                cueStack.AddCue(cue);
            }
            else
            {
                var cue = new Cue(dmxDataMap, name);
                cueStack = new CueStack(cue);
            }
        }
        private void SaveData()
        {
            //var data = JsonUtility.ToJson(cueStack);
            string json = JsonConvert.SerializeObject(cueStack, Formatting.Indented);
            System.IO.File.WriteAllText("Assets/Scripts/ScriptableObjects/" + "/presets.json", json);
            Debug.Log("STACK IS SAVED");
        }
        void ReadData()
        {
            string json = System.IO.File.ReadAllText("Assets/Scripts/ScriptableObjects/" + "/presets.json");
            var stack = JsonConvert.DeserializeObject<CueStack>(json);
            cueStack = new CueStack();
            cueStack = stack;
        }
    }
}
