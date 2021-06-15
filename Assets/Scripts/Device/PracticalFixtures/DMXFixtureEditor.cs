using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IA;

[CustomEditor(typeof(GenericDMXFixture))]
public class DMXFixtureEditor : Editor
{
    bool show = true;
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        var content = (GenericDMXFixture)target;
        var channelFunctions = content.getChannelFunctions;
        var dmxDataMap = content.artNetData.dmxDataMap;
        int addr = content.getDmxAddress;
        int universe = content.getUniverse;
        bool standAlone = content.isStandAlone;
        show = EditorGUILayout.BeginFoldoutHeaderGroup(show, "Sliders");
        if (standAlone)
        {
            if (show)
            {
                foreach (var item in channelFunctions)
                {
                    GUILayout.Label(item.Key);
                    int i = EditorGUILayout.IntSlider(dmxDataMap[universe - 1][addr + item.Value - 1], 0, 255);
                    if (i != dmxDataMap[universe - 1][addr + item.Value])
                    {
                        dmxDataMap[universe - 1][addr + item.Value - 1] = (byte)i;
                    }
                    content.artNetData.dmxUpdate.Invoke();
                }
            }


        }
    }
}
