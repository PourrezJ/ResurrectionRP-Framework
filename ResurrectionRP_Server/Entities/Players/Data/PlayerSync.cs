using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public class PlayerSync
    {
        public bool IsDead { get; set; }
        public bool IsCuff { get; set; }
        public string WalkingAnim { get; set; }
        public AnimationsSync AnimationsSync { get; set; }
        public string MoodAnim { get; set; }
        public bool Crounch { get; set; }
        public bool Freeze { get; set; }
        public bool Injured { get; set; }
    }
}
