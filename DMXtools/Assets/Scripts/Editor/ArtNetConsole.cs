using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ArtNet.Sockets;
using ArtNet.Packets;

namespace IA
{
    [ExecuteAlways]
    public class ArtNetConsole : EditorWindow
    {
        ArtNetData artNetData;
        Rect header;
        Rect side;
        Rect body;
        Rect subHeader;
        DMX dMX;
        ArtNetSocket artnet;
        IPEndPoint remote;
        DMXFixture[] heads;
        List<DMXFixture> selectedHeadObjects = new List<DMXFixture>();
        GroupController groupController;
        private int dmxUniverse = 7;
        
        bool sendDMX = false;
        string remoteIP = "localhost";

        private int activeUniverse;
        int activeView;
        int numberOfColumns = 3;
        int numberOfHeads;
        bool[] receiveArtNet = new bool[8];
        string[] universes = { "Universe 1", "Universe 2", "Universe 3", "Universe 4", "Universe 5", "Universe 6", "Universe 7", "Universe 8" };



        Vector2 scroll = new Vector2();

        [MenuItem("Window/ArtNet/Console")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:

            ArtNetConsole window = (ArtNetConsole)EditorWindow.GetWindow(typeof(ArtNetConsole));
            window.titleContent.text = "ArtNet Console";
            window.Show();
        }
        void OnEnable()
        {
            artnet = new ArtNetSocket();
            remote = new IPEndPoint(FindFromHostName(remoteIP), ArtNetSocket.Port);
            artnet.Open(FindFromHostName("localhost"), null);
            FindDataMap();
            Debug.Log("artnet socket");
            artNetData.dmxUpdate.AddListener(Repaint);
            ArtnetReceiver(CallUpdate);



        }
        void Update()
        {
            //Debug.Log("updating");
            //update real DMX universe

        }
        void OnInspectorUpdate()
        {
            Repaint();
        }
        void OnDisable()
        {
            artnet.Close();
            SaveDataMap();
            if (dMX != null)
            {
                dMX.Quit();
                dMX = null;
            }
        }


        void OnGUI()
        {
            DrawLayouts();
            DrawHeader();
            DrawSidePanel();
            DrawBody();
        }
        void CallUpdate()
        {
            Debug.Log("update is called");
            artNetData.dmxUpdate.Invoke();
        }
        void DrawLayouts()
        {
            header.x = 0;
            header.y = 0;
            header.width = Screen.width;
            header.height = 40;


            side.x = 0;
            side.y = header.height;
            side.width = 400;
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
            activeUniverse = GUILayout.SelectionGrid(activeUniverse, universes, 8);
            EditorGUILayout.BeginHorizontal();
            bool setAsDmx = EditorGUILayout.Toggle("DMX out universe", dmxUniverse == activeUniverse);
            if (setAsDmx && dMX != null)
            {
                dmxUniverse = activeUniverse;
                bool sendDMX = EditorGUILayout.Toggle("serial DMX", false);
                if (sendDMX && dMX == null)
                {
                    dMX = new DMX();
                }
            }
            else
            {
                receiveArtNet[activeUniverse] = EditorGUILayout.Toggle("receive ArtNet", receiveArtNet[activeUniverse]);
                if (GUILayout.Button("Update"))
                {
                    CallUpdate();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void DrawSidePanel()
        {
            GUILayout.BeginArea(side);
            
            if (NumberOfSelected() > 0)
            {
                if (groupController == null || NumberOfSelected() != selectedHeadObjects.Count)
                {
                    selectedHeadObjects = new List<DMXFixture>();
                    foreach(var head in heads)
                    {
                        if(head.selected)
                            selectedHeadObjects.Add(head);
                    }
                    //EditorGUILayout.LabelField(""+selectedHeadObjects.Count);
                    groupController = new GroupController(selectedHeadObjects);
                }
                EditorGUILayout.BeginVertical();
                foreach (var head in selectedHeadObjects)
                {
                    EditorGUILayout.LabelField(""+head.getDmxAddress);
                }
                foreach (KeyValuePair<string, List<int>> channelFunction in groupController.map)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(channelFunction.Key , GUILayout.Width(100));
                    int i = EditorGUILayout.IntSlider(groupController.level[channelFunction.Key], 0, 255, GUILayout.Width(300));
                    groupController.level[channelFunction.Key] = i;

                    foreach (int channel in channelFunction.Value)
                    {
                        if (i != artNetData.dmxDataMap[activeUniverse][channel])
                        {
                            artNetData.dmxDataMap[activeUniverse][channel] = (byte)i;
                            if (sendDMX & activeUniverse == dmxUniverse)
                            {
                                dMX[channel] = (byte)i;
                            }
                            else
                            {
                                artNetData.dmxUpdate.Invoke();
                            }
                        }

                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }
        void DrawBody()
        {
            string[] views = { "channels", "heads" };
            GUILayout.BeginArea(body);
            EditorGUILayout.BeginHorizontal();

            activeView = GUILayout.SelectionGrid(activeView, views, 2);
            EditorGUILayout.EndHorizontal();
            if (activeView == 1)
            {
                DrawHeads();
            }
            else
            {
                if (receiveArtNet[activeUniverse])
                    DrawMap();
                else
                    DrawSliders();
            }

            GUILayout.EndArea();
        }
        void ArtnetReceiver(Action callback)
        {
            artnet.NewPacket += (object sender, ArtNet.Sockets.NewPacketEventArgs<ArtNet.Packets.ArtNetPacket> e) =>
            {
                if (e.Packet.OpCode == ArtNet.Enums.ArtNetOpCodes.Dmx)
                {
                    var packet = e.Packet as ArtNetDmxPacket;


                //Debug.Log("new packet");

                var universe = packet.Universe;
                //Debug.Log("universe: " + receiveArtNet[universe]);
                if (receiveArtNet[universe])
                    {
                        artNetData.SetData(universe, packet.DmxData);
                    //CallUpdate();
                }




                }
                callback();
            };
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

        private void ReadDataMap()
        {
            //dmx = artNetData.dmxDataMap;

        }

        private void SaveDataMap()
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(artNetData);
            AssetDatabase.SaveAssets();
            Debug.Log("saved");
        }
        void DrawSliders()
        {
            int numberOfChannels = 512;

            //Levels
            //DrawDropdown(new Rect(20,20,30,20), new GUIContent("test"));
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUIStyle labelWidth = new GUIStyle();
            labelWidth.fixedWidth = 300;
            for (int f = 0; f < numberOfChannels; f++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("[Channel " + (f + 1) + "] ", GUILayout.Width(100));
                int i = EditorGUILayout.IntSlider((int)artNetData.dmxDataMap[activeUniverse][f], 0, 255, GUILayout.Width(300));
                if (i != artNetData.dmxDataMap[activeUniverse][f])
                {
                    artNetData.dmxDataMap[activeUniverse][f] = (byte)i;
                    if (sendDMX & activeUniverse == dmxUniverse)
                    {
                        dMX[f] = (byte)i;
                    }
                    else
                    {
                        artNetData.dmxUpdate.Invoke();
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawHeads()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Heads"))
            {
                heads = FindHeads().ToArray();
                numberOfHeads = heads.Length;
                
            }
            if (GUILayout.Button("Connect Heads"))
            {
                heads = ConnectHeads();
                
                
            }
            if (GUILayout.Button("Reset selection"))
            {
                ResetSelection();
               
            }
            EditorGUILayout.EndHorizontal();


            //Debug.Log(numberOfHeads);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("number of patched heads :" + numberOfHeads, GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("Number of Columns", GUILayout.MaxWidth(120));
            numberOfColumns = EditorGUILayout.IntSlider(numberOfColumns, 1, 10, GUILayout.Width(150));

            EditorGUILayout.EndHorizontal();


            


            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int f = 0; f < numberOfHeads / numberOfColumns + 1; f++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < numberOfColumns; j++)
                {
                    //Debug.Log("j"+j);
                    int currentHead = f * numberOfColumns + j;
                    if (currentHead >= numberOfHeads)
                        continue;
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(heads[currentHead].name, GUILayout.MaxWidth(100));
                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(40));
                    EditorGUILayout.LabelField("Start CH: " + heads[currentHead].GetComponent<DMXFixture>().getDmxAddress, GUILayout.MaxWidth(70));
                    EditorGUILayout.LabelField("Channels: " + heads[currentHead].GetComponent<DMXFixture>().getNumberOfChannels, GUILayout.MaxWidth(100));
                    heads[currentHead].selected = EditorGUILayout.Toggle(heads[currentHead].selected);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        int NumberOfSelected()
        {
            if(heads==null)
                return 0;
            int count = 0;
            foreach(var head in heads)
                if (head.selected)
                    count++;
            return count;
        }
        int NumberOfSliders()
        {
            int count = 0;
            for (int i = 0; i < NumberOfSelected(); i++)
            {

            }
            return count;
        }
        private DMXFixture[] ConnectHeads()
        {
            DMXFixture[] heads = GameObject.FindObjectsOfType<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
            }
            return heads;
        }
        private List<DMXFixture> FindHeads()
        {
            DMXFixture[] allHeads = GameObject.FindObjectsOfType<DMXFixture>();
            List<DMXFixture> headsOfUniverse = new List<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
                if(heads[i].getUniverse == activeUniverse)
                    headsOfUniverse.Add(heads[i]);
            }
            return headsOfUniverse;
        }
        void ResetSelection()
        {
            DMXFixture[] heads = GameObject.FindObjectsOfType<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
                heads[i].selected = false;
            }
            
        }
        void DrawMap()
        {

            int numberOfChannels = 512;

            //Levels
            //DrawDropdown(new Rect(20,20,30,20), new GUIContent("test"));
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUIStyle labelWidth = new GUIStyle();
            labelWidth.fixedWidth = 300;
            for (int f = 0; f < (numberOfChannels / 8); f++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < 8; j++)
                    EditorGUILayout.LabelField("Ch" + (f * 8 + j + 1) + "@" + artNetData.dmxDataMap[activeUniverse][f * 8 + j], GUILayout.Width(100));

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        static IPAddress FindFromHostName(string hostname)
        {
            var address = IPAddress.None;
            try
            {
                if (IPAddress.TryParse(hostname, out address))
                    return address;

                var addresses = Dns.GetHostAddresses(hostname);
                for (var i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        address = addresses[i];
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat(
                    "Failed to find IP for :\n host name = {0}\n exception={1}",
                    hostname, e);
            }
            return address;
        }

    }
}