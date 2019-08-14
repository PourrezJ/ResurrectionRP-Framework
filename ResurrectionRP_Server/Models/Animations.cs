using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Models
{
    public class Animation
    {
        public string Name;
        public string Description;
        public string AnimDict;
        public string AnimName;

        public Animation(string Name, string Description, string AnimDict, string AnimName)
        {
            this.Name = Name;
            this.Description = Description;
            this.AnimDict = AnimDict;
            this.AnimName = AnimName;
        }
    }
}
