using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        public static void Initialize()
        {
            Alt.OnColShape += OnEntityColshape;
            Alt.OnServerCustomEvent += Alt_OnServerCustomEvent;
            AltAsync.OnClient("InteractionInColshape", OnEntityInteractInColShape);
        }

        private static void Alt_OnServerCustomEvent(string eventName, ref AltV.Net.Native.MValueArray mValueArray)
        {

        }
    }
}
