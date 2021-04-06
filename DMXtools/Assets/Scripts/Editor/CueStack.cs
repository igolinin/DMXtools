using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace IA
{
    [System.Serializable]
    public class CueStack
    {
        [SerializeField]
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
        [SerializeField]
        public string name;
        [SerializeField]
        public byte[][] cueData;
        public Cue(byte[][] cueData, string name)
        {
            this.name = name;
            this.cueData = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                this.cueData[i] = new byte[/* data[i].Length */512];
                //Array.Copy(data[i], cueData[i],  512);
                for(int j = 0; j < 512; j++)
                {
                    this.cueData[i][j] = (byte)cueData[i][j];
                }
            }
                
        }
    }
}
