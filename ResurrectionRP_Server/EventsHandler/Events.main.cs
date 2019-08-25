using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.EventsHandler
{
    public partial class Events
    {
        public Events()
        {
            Alt.OnColShape += OnEntityColshape;

            Alt.OnServerCustomEvent += Alt_OnServerCustomEvent;
        }

        private void Alt_OnServerCustomEvent(string eventName, ref AltV.Net.Native.MValueArray mValueArray)
        {

        }
    }
}
