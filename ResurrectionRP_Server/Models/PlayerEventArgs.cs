﻿using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Models
{
    public class PlayerEventArgs : System.EventArgs
    {
        public IPlayer Player { get; }

        internal PlayerEventArgs(IPlayer player)
        {
            Player = player;
        }
    }
}
