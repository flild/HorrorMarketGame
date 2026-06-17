using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interfaces
{
    public struct PromptData
    {
        public string Key;
        public object[] Args;

        public PromptData(string key, params object[] args)
        {
            Key = key;
            Args = args;
        }
    }
}
