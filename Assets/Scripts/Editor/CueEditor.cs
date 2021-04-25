using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
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

        [MenuItem("Window/Art-net/Cue Editor")]
        static void Init()
        {
            CueEditor window = (CueEditor)EditorWindow.GetWindow(typeof(CueEditor));
            window.titleContent.text = "Cue Editor";
            window.Show();
        }
        void OnEnable()
        {
            FindDataMap();
            ReadData(SceneManager.GetActiveScene());
            EditorSceneManager.sceneOpened += ChangedActiveScene;
        }
        void OnDisable()
        {
            SaveData(SceneManager.GetActiveScene());
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
            if (GUILayout.Button("Save Stack"))
            {
                //ClearOutput();
                SaveData(SceneManager.GetActiveScene());
            }
            if (GUILayout.Button("Clear Stack"))
            {
                cueStack.ClearStack();
            }
            playback = EditorGUILayout.ToggleLeft("Playback", playback, GUILayout.Width(150));



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
            if (GUILayout.Button("Remove Selected"))
            {
                RemoveCue();
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
            if ( PlayerPrefs.GetInt("survey") != 1 )
            {
                var redTextstyle = new GUIStyle(GUI.skin.button);
                redTextstyle.normal.textColor = Color.red;
                if (GUILayout.Button("This button will go away after \nyou click it to answer my questions.", redTextstyle, GUILayout.Height(60)))
                {
                    Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSccgEIekGKbzOA9qYSg5l_A_0poEjEH9mMbKafyqQVrVC0vtQ/viewform?usp=sf_link");
                    PlayerPrefs.SetInt("survey", 1);
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
        void RemoveCue()
        {
            cueStack.RemoveCue(currentCue);
            if (currentCue > 0)
                currentCue--;
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
        private void SaveData(Scene scene)
        {
            //var data = JsonUtility.ToJson(cueStack);
            string json = JsonConvert.SerializeObject(cueStack, Formatting.Indented);
            string path = PresetPath(scene);
            System.IO.File.WriteAllText(path, json);
        }
        string PresetPath(Scene scene)
        {
            string sceneName = scene.name;
            string path = "Assets/Presets/CueStacks/" + sceneName + ".json";
            return path;
        }
        void ReadData(Scene scene)
        {
            if (System.IO.File.Exists(PresetPath(scene)))
            {
                string json = System.IO.File.ReadAllText(PresetPath(scene));
                var stack = JsonConvert.DeserializeObject<CueStack>(json);
                cueStack = new CueStack();
                cueStack = stack;
            }else
            {
                cueStack = new CueStack();
            }

        }
        private void ChangedActiveScene(Scene next, OpenSceneMode mode = OpenSceneMode.Single)
    {
        //SaveData(current);
        ReadData(next);
        Debug.Log("Scene changed");
    }
    }
}
