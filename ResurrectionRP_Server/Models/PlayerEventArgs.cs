﻿using System;
using System.Collections.Generic;
using System.Text;

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
