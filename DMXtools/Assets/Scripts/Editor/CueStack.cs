using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace IA
{
    [System.Serializable]
    public class CueStack
    {
        public List<Cue> stack;
        public CueStack()
        {
            stack = new List<Cue>();
        }
        public CueStack(Cue cue)
        {
            stack = new List<Cue>();
            stack.Add(cue);
        }
        public void AddCue(Cue cue)
        {
            stack.Add(cue);
        }
        public Cue GetCue(int number)
        {
            
            return stack[number];
        }
        public void ClearStack()
        {
            stack = new List<Cue>();
        }
    }
     [System.Serializable]
    public class Cue
    {
        public string name;
        public byte[][] cueData;
        public Cue(byte[][] data, string cueName)
        {
            name = cueName;
            cueData = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                cueData[i] = new byte[data[i].Length];
                Array.Copy(data[i], cueData[i],  data[i].Length);
            }
                
        }
    }
}
