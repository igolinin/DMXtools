using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace IA
{
    public class GroupController
    {

        public Dictionary<string, DeviceGroup> map;
        
        public GroupController(List<DMXFixture> devices)
        {
            map = new Dictionary<string, DeviceGroup>();
            
            foreach(DMXFixture device in devices)
            {
                AddSelected(device);
            }
        }
        public GroupController(DMXFixture device)
        {
            map = new Dictionary<string, DeviceGroup>();
            AddSelected(device);
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
                    map[channelFunction.Key].devices.Add(device);
                }
                else
                {
                    //map.Add(channelFunction.Key, new List<int[]>{new int[]{device.getUniverse, device.getDmxAddress + channelFunction.Value}});
                    var newDeviceGroup = new DeviceGroup(device);
                    map.Add(channelFunction.Key, newDeviceGroup);
                }
            }
            
            device.added = true;
        }
        public void RemoveDeselected(DMXFixture device)
        {
            List<string> keysToRemove = new List<string>();
            device.added = false;
            foreach (KeyValuePair<string, int> channelFunction in device.getChannelFunctions)
            {
                if (map.ContainsKey(channelFunction.Key))
                {
                    map[channelFunction.Key].devices.RemoveAll(x => x == device);
                    if(map[channelFunction.Key].devices.Count == 0)
                        keysToRemove.Add(channelFunction.Key);
                }
            }
            foreach(string key in keysToRemove)
            {
                map.Remove(key);
            }
        }
    }
    public class DeviceGroup
    {
        public int level;
        public List<DMXFixture> devices;
        public DeviceGroup(DMXFixture newDevice)
        {
            devices = new List<DMXFixture>();
            devices.Add(newDevice);
            level = 0;
        }
        public DeviceGroup(DMXFixture newDevice, int newLevel)
        {
            devices = new List<DMXFixture>();
            devices.Add(newDevice);
            level = newLevel;
        }
    }
}
