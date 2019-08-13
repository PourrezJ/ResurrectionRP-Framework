using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Models
{
    public class PlayerRemoteEventEventArgs : PlayerEventArgs
    {
        public string EventName { get; }
        public IReadOnlyList<object> Arguments { get; }

        internal PlayerRemoteEventEventArgs(IPlayer player, string eventName, object[] arguments) : base(player)
        {
            EventName = eventName;
            Arguments = arguments;
        }
    }
}
