using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA
{
    public class GroupController
    {

        public Dictionary<string, List<int[]>> map;
        public Dictionary<string, int> level;

        public GroupController(List<DMXFixture> devices)
        {
            map = new Dictionary<string, List<int[]>>();
            level = new Dictionary<string, int>();
            foreach(DMXFixture device in devices)
            {
                AddSelected(device);
            }
        }
        public GroupController(DMXFixture device)
        {
            map = new Dictionary<string, List<int[]>>();
            level = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> channelFunction in device.getChannelFunctions)
            {
                int[] newDevice = new int[2]{device.getUniverse, device.getDmxAddress + channelFunction.Value};
                var newList = new List<int[]>();
                newList.Add(newDevice);
                map.Add(channelFunction.Key, newList);
                level.Add(channelFunction.Key, 0);
            }
            device.added = true;
        }
        public int getSize()
        {
            return map.Count;
        }
        public void AddSelected(DMXFixture device)
        {
            foreach (KeyValuePair<string, int> channelFunction in device.getChannelFunctions)
            {
                if (map.ContainsKey(channelFunction.Key))
                {
                    map[channelFunction.Key].Add(new int[]{device.getUniverse, device.getDmxAddress + channelFunction.Value});
                }
                else
                {
                    map.Add(channelFunction.Key, new List<int[]>{new int[]{device.getUniverse, device.getDmxAddress + channelFunction.Value}});
                    level.Add(channelFunction.Key, 0);
                }
            }
            device.added = true;
        }
        public void RemoveDeselected(DMXFixture device)
        {
            foreach (KeyValuePair<string, int> channelFunction in device.getChannelFunctions)
            {
                
                    map[channelFunction.Key].RemoveAll(x => x[0] == device.getUniverse && x[1] == device.getDmxAddress + channelFunction.Value);
               
            }
                device.added = false;
        }
    }
}
