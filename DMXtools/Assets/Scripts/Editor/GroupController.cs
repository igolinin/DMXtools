using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA
{
    public class GroupController
    {

        public Dictionary<string, List<int>> map;
        public Dictionary<string, int> level;

        public GroupController(List<DMXFixture> devices)
        {
            map = new Dictionary<string, List<int>>();
            level = new Dictionary<string, int>();
            foreach(DMXFixture device in devices)
            {
                AddSelected(device);
            }
        }
        public GroupController(DMXFixture device)
        {
            map = new Dictionary<string, List<int>>();
            level = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> channelFunction in device.getChannelFunctions)
            {
                map.Add(channelFunction.Key, new List<int>(device.getDmxAddress + channelFunction.Value));
                level.Add(channelFunction.Key, 0);
            }
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
                    map[channelFunction.Key].Add(device.getDmxAddress + channelFunction.Value);
                }
                else
                {
                    map.Add(channelFunction.Key, new List<int>(){device.getDmxAddress + channelFunction.Value});
                    level.Add(channelFunction.Key, 0);
                }
            }
        }
    }
}
