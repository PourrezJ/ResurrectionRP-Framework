using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.EventHandlers
{

    public static partial class Events
    {

        #region Delegates
        public delegate void EmergencyCallEmit(IPlayer client, string factionName, Vector3 position, string reason);
        public delegate Task EmergencyCallPlayerState(IPlayer client, string FactionName,bool state);
        public delegate void EmergencyCall(IPlayer client, int call);
        public delegate void EmergencyCallMenu(IPlayer client);
        #endregion

        #region Public events 
        public static event EmergencyCallEmit OnPlayerEmitEmergencyCall;
        public static event EmergencyCallPlayerState OnPlayerChangeStateEmergencyCall;
        public static event EmergencyCall OnPlayerReleaseEmergencyCall;
        public static event EmergencyCall OnPlayerRemoveEmergencyCall;
        public static event EmergencyCallMenu OnPlayerMenuEmergencyCall;
        #endregion


        /**
         * InteractEmergencyCall
         * Emit format: {
         *                  reason: string,
         *                  factionId: string,
         *                  reason: string
         *              }
         *  Take format: {
         *              }
         *  Release format: {
         *              }
         **/
        public static void InteractEmergencyCall(IPlayer client, string reason, string data, string data2)
        {
            if (!client.Exists)
                return;

            switch (reason)
            {

                case "emit":
                    OnPlayerEmitEmergencyCall?.Invoke(client, data, client.Position.ConvertToVector3(), data2);
                    Phone.PhoneManager.ClosePhone(client);
                    break;


                case "release":
                    OnPlayerReleaseEmergencyCall?.Invoke(client, int.Parse(data));
                    break;

                case "state":
                    OnPlayerChangeStateEmergencyCall?.Invoke(client, data, bool.Parse(data2));
                    break;

                case "remove":
                    OnPlayerRemoveEmergencyCall?.Invoke(client, int.Parse(data));
                    break;

                case "openMenu":
                    OnPlayerMenuEmergencyCall?.Invoke(client);
                    break;

                default:
                    Alt.Server.LogError("Unknown Interact Emergency Call parameter " + reason);
                    break;
            }
        }

        public static void InvokeEmergencyCallState(IPlayer client, string faction,bool state) =>
            OnPlayerChangeStateEmergencyCall?.Invoke(client, faction,state);
    }
}