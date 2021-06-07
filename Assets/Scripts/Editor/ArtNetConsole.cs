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
        Dictionary<int, DMXFixture[]> heads;
        List<DMXFixture> selectedHeadObjects = new List<DMXFixture>();
        GroupController groupController;
        const int numberOfUniverses = 8;
        private int dmxUniverse = numberOfUniverses + 1;
        int[] patchSize = new int[numberOfUniverses + 1];

        bool sendDMX;
        string remoteIP = "localhost";

        private int activeUniverse;
        int activeView;
        int numberOfColumns = 3;
        int numberOfHeads;
        bool[] receiveArtNet = new bool[numberOfUniverses + 1];
        string[] views = { "Channels", "Heads" };
        string[] universes = { "All", "Universe 1", "Universe 2", "Universe 3", "Universe 4", "Universe 5", "Universe 6", "Universe 7", "Universe 8" };



        Vector2 scroll = new Vector2();

        [MenuItem("Window/Art-net/Console")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:

            ArtNetConsole window = (ArtNetConsole)EditorWindow.GetWindow(typeof(ArtNetConsole));
            window.titleContent.text = "ArtNet Console";
            window.Show();
        }

        void OnEnable()
        {
            FindDataMap();
            FindAllHeads();
            artNetData.dmxUpdate.AddListener(Repaint);
            OpenArtNet();

        }
        void OpenArtNet()
        {
            if (artnet != null)
                artnet.Close();
            artnet = new ArtNetSocket();
            remote = new IPEndPoint(FindFromHostName(remoteIP), ArtNetSocket.Port);
            //FindFromHostName("192.168.1.124")
            artnet.Open(IPAddress.Any, null);
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
            //Debug.Log("update is called");
            artNetData.dmxUpdate.Invoke();
        }
        void DrawLayouts()
        {
            header.x = 0;
            header.y = 0;
            header.width = Screen.width;
            header.height = 65;
            
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
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Full Blackout", GUILayout.Width(150)) )
            {
                ResetSelection();
                ResetGroupController();
                ClearOutput();
            }
            if (GUILayout.Button("Find Heads", GUILayout.Width(150)))
            {
                heads = FindAllHeads();
            }
            
            if (GUILayout.Button("Reset Selection", GUILayout.Width(150)))
            {
                ResetSelection();
                ResetGroupController();
            }
            if (GUILayout.Button("Select All", GUILayout.Width(150)) )
            {
                SelectAll();
            }
            EditorGUILayout.EndHorizontal();
            activeUniverse = GUILayout.SelectionGrid(activeUniverse, universes, 9);
            EditorGUILayout.BeginHorizontal();
            
            if (activeUniverse != 0)
            {
                bool setAsDmx = EditorGUILayout.ToggleLeft("DMX Out Universe", dmxUniverse == activeUniverse, GUILayout.Width(150));
                if (setAsDmx)
                {
                    dmxUniverse = activeUniverse;
                    sendDMX = EditorGUILayout.ToggleLeft("Serial DMX", sendDMX);
                    if (sendDMX && dMX == null)
                    {
                        dMX = new DMX(dmxUniverse - 1);
                    }
                    if (!sendDMX && dMX != null)
                    {
                        dMX.Quit();
                        dMX = null;
                    }
                }
                /* remoteIP = GUILayout.TextField(remoteIP);
                if(GUILayout.Button("change",GUILayout.Width(60)))
                {
                    OpenArtNet();
                } */
                receiveArtNet[activeUniverse - 1] = EditorGUILayout.ToggleLeft("Receive Art-net", receiveArtNet[activeUniverse - 1]);
                if (GUILayout.Button("Update Art-net Data", GUILayout.Width(150)))
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
            if (NumberOfSelected() == 0)
            {
                groupController = null;
                selectedHeadObjects = new List<DMXFixture>();
                ResetSelection();
            }

            if (NumberOfSelected() > 0 && heads != null && activeView == 1)
            {
                if (groupController == null)
                {
                    selectedHeadObjects = new List<DMXFixture>();
                    foreach (var head in heads[activeUniverse])
                    {
                        if (head.selected)
                        {
                            selectedHeadObjects.Add(head);
                            Selection.activeGameObject = head.gameObject;
                        }
                    }
                    //EditorGUILayout.LabelField(""+selectedHeadObjects.Count);
                    groupController = new GroupController(selectedHeadObjects, artNetData);
                }
                else if (/* NumberOfSelected() != selectedHeadObjects.Count */true)
                {

                    foreach (var head in heads[activeUniverse])
                    {
                        if (head.selected && !head.added)
                        {
                            groupController.AddSelected(head, artNetData);
                            Selection.activeGameObject = head.gameObject;
                            selectedHeadObjects.Add(head);
                        }
                        else
                        if (head.selected == false && head.added == true)
                        {
                            groupController.RemoveDeselected(head);
                            selectedHeadObjects.Remove(head);
                        }

                    }
                }
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("Selected:");
                foreach (var head in selectedHeadObjects)
                {
                    EditorGUILayout.LabelField("" + head.getDmxAddress, GUILayout.Width(20));
                }
                EditorGUILayout.EndHorizontal();
                foreach (KeyValuePair<string, DeviceGroup> channelFunction in groupController.map)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(channelFunction.Key, GUILayout.Width(100));
                    int i = EditorGUILayout.IntSlider(groupController.map[channelFunction.Key].level, 0, 255, GUILayout.Width(300));
                    groupController.map[channelFunction.Key].level = i;

                    foreach (DMXFixture device in channelFunction.Value.devices)
                    {
                        if (i != artNetData.dmxDataMap[device.getUniverse - 1][device.getDmxAddress - 1 + device.getChannelFunctions[channelFunction.Key]])
                        {
                            artNetData.dmxDataMap[device.getUniverse - 1][device.getDmxAddress - 1 + device.getChannelFunctions[channelFunction.Key]] = (byte)i;
                            if (sendDMX & activeUniverse == dmxUniverse)
                            {
                                dMX[device.getDmxAddress - 1 + device.getChannelFunctions[channelFunction.Key] + 1] = (byte)i;
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

            GUILayout.BeginArea(body);
            EditorGUILayout.BeginHorizontal();

            activeView = GUILayout.SelectionGrid(activeView, views, 2);
            EditorGUILayout.EndHorizontal();
            if (activeUniverse != 0 && receiveArtNet[activeUniverse - 1])
                    DrawMap();
            else
            {
                if (activeView == 1)
                {
                    if (heads == null)
                        FindAllHeads();
                    DrawHeads();
                }
                else
                {
                    DrawSliders();

                }
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
                    var universe = packet.Universe;
                    //Debug.Log("universe: " + universe);
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
        }
        void DrawSliders()
        {
            if (activeUniverse != 0)
            {
                int numberOfChannels = 512;
                scroll = EditorGUILayout.BeginScrollView(scroll);
                GUIStyle labelWidth = new GUIStyle();
                labelWidth.fixedWidth = 300;
                for (int f = 0; f < numberOfChannels; f++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("[Channel " + (f + 1) + "] ", GUILayout.Width(100));
                    int i = EditorGUILayout.IntSlider((int)artNetData.dmxDataMap[activeUniverse - 1][f], 0, 255, GUILayout.Width(300));
                    if (i != artNetData.dmxDataMap[activeUniverse - 1][f])
                    {
                        artNetData.dmxDataMap[activeUniverse - 1][f] = (byte)i;
                        if (sendDMX & activeUniverse == dmxUniverse)
                        {
                            dMX[f + 1] = (byte)i;
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

        }
        void DrawHeads()
        {
           


            if (heads != null && patchSize != null )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Number of Heads :" + patchSize[activeUniverse], GUILayout.MaxWidth(200));
                EditorGUILayout.LabelField("Number of Columns", GUILayout.MaxWidth(120));
                numberOfColumns = EditorGUILayout.IntSlider(numberOfColumns, 1, 10, GUILayout.Width(150));

                EditorGUILayout.EndHorizontal();

                scroll = EditorGUILayout.BeginScrollView(scroll);

                for (int f = 0; f < patchSize[activeUniverse] / numberOfColumns + 1; f++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < numberOfColumns; j++)
                    {
                        //
                        try
                        {
                            int currentHead = f * numberOfColumns + j;
                            if (currentHead >= patchSize[activeUniverse])
                                continue;
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField(heads[activeUniverse][currentHead].name, GUILayout.MaxWidth(120));
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(40));
                            EditorGUILayout.LabelField("Address: ", GUILayout.MaxWidth(50));
                            int dmxAddr = EditorGUILayout.IntField(heads[activeUniverse][currentHead].GetComponent<DMXFixture>().getDmxAddress, GUILayout.MaxWidth(30));
                            if (dmxAddr != heads[activeUniverse][currentHead].GetComponent<DMXFixture>().getDmxAddress)
                                heads[activeUniverse][currentHead].GetComponent<DMXFixture>().dmxAddress = dmxAddr;
                            if (GUILayout.Button("auto", GUILayout.MaxWidth(40)))
                            {
                                AutoPatch(heads[activeUniverse][currentHead]);
                            }
                            EditorGUILayout.LabelField( heads[activeUniverse][currentHead].GetComponent<DMXFixture>().getNumberOfChannels +" Channels" , GUILayout.MaxWidth(70));
                            heads[activeUniverse][currentHead].selected = EditorGUILayout.Toggle(heads[activeUniverse][currentHead].selected);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                        }
                        catch
                        {
                            heads = FindAllHeads();
                        }

                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }


        }
        void AutoPatch(DMXFixture device)
        {
            var maxAddressedHead = heads[activeUniverse].OrderByDescending(head => head.getDmxAddress).First();
            device.GetComponent<DMXFixture>().dmxAddress = maxAddressedHead.getDmxAddress + maxAddressedHead.getNumberOfChannels;
        }
        int NumberOfSelected()
        {
            DMXFixture[] allHeads = GameObject.FindObjectsOfType<DMXFixture>();
            if (allHeads == null)
                return 0;
            int count = 0;
            foreach (var head in allHeads)
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
        private void ConnectHeads()
        {
            DMXFixture[] heads = GameObject.FindObjectsOfType<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
            }

        }
        private List<DMXFixture> FindHeadsOfUniverse()
        {
            DMXFixture[] allHeads = GameObject.FindObjectsOfType<DMXFixture>();
            List<DMXFixture> headsOfUniverse = new List<DMXFixture>();
            for (int i = 0; i < allHeads.Length; i++)
            {
                allHeads[i].FindDataMap();
                if (allHeads[i].getUniverse == activeUniverse)
                    headsOfUniverse.Add(allHeads[i]);
            }
            return headsOfUniverse;
        }
        private Dictionary<int, DMXFixture[]> FindAllHeads()
        {
            DMXFixture[] heads = GameObject.FindObjectsOfType<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
            }
            Dictionary<int, DMXFixture[]> sortedHeads = new Dictionary<int, DMXFixture[]>();
            patchSize = new int[numberOfUniverses + 1];
            for (int i = 0; i < heads.Length; i++)
            {
                patchSize[heads[i].getUniverse]++;
                patchSize[0]++;
            }
            sortedHeads[0] = heads;
            for (int i = 1; i <= numberOfUniverses; i++)
            {
                var tempHeads = heads.Where(h => h.getUniverse == i).ToList();
                var ordered = tempHeads.OrderBy(x => x.getDmxAddress);
                sortedHeads[i] = ordered.ToArray();
            }
            return sortedHeads;
        }
        void ResetSelection()
        {
            DMXFixture[] heads = GameObject.FindObjectsOfType<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
                heads[i].selected = false;
                heads[i].added = false;
            }

        }
        void SelectAll()
        {
            DMXFixture[] heads = GameObject.FindObjectsOfType<DMXFixture>();
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].FindDataMap();
                heads[i].selected = true;
                heads[i].added = false;
            }
        }
        void ResetGroupController()
        {
            groupController = null;
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
                    EditorGUILayout.LabelField("Ch" + (f * 8 + j + 1) + "@" + artNetData.dmxDataMap[activeUniverse - 1][f * 8 + j], GUILayout.Width(100));

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        void ClearOutput()
        {
            artNetData.ResetData();
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