using System;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Farms;

namespace ResurrectionRP_Server.EventHandlers
{ 
    public static partial class Events
    {

        #region Delegates
        public delegate void MiningInteract(IPlayer client, MiningPoint miningPoint);
        #endregion

        #region Public events 
        public static event MiningInteract OnPlayerEmitEmergencyCall;
        #endregion

        public static void InteractMining()
        {

        }
    }
}
