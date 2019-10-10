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
        public delegate Task EmergencyCallEmitAsync(IPlayer client, string factionName, Vector3 position, string reason);
        public delegate Task EmergencyCallPlayerState(IPlayer client, string factionName, bool state);
        public delegate void EmergencyCall(IPlayer client, string factionName, int call);
        #endregion

        #region Public events 
        public static event EmergencyCallEmitAsync OnPlayerEmitEmergencyCall;
        public static event EmergencyCallPlayerState OnPlayerChangeStateEmergencyCall;
        public static event EmergencyCall OnPlayerTakeEmergencyCall;
        public static event EmergencyCall OnPlayerReleaseEmergencyCall;
        public static event EmergencyCall OnPlayerRemoveEmergencyCall;
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
        public static void InteractEmergencyCall(IPlayer client, object[] args)
        {
            var reason = args[0];
            switch (reason)
            {

                case "emit":
                    OnPlayerEmitEmergencyCall.Invoke(client,  args[1].ToString(), client.Position.ConvertToVector3(), args[2].ToString());
                    break;

                case "take":
                    OnPlayerTakeEmergencyCall?.Invoke(client, args[1].ToString(), (int)args[2]);
                    break;

                case "release":
                    OnPlayerReleaseEmergencyCall?.Invoke(client, args[1].ToString(), (int)args[2]);
                    break;

                case "state":
                    OnPlayerChangeStateEmergencyCall?.Invoke(client, args[1].ToString(), (bool)args[2]);
                    break;

                case "remove":
                    OnPlayerRemoveEmergencyCall?.Invoke(client, args[1].ToString(), (int)args[2]);
                    break;

                default:
                    Alt.Server.LogError("Unknown Interact Emergency Call parameter " + reason);
                    break;
            }
        }

        public static void InvokeEmergencyCallState(IPlayer client, string factionName, bool state) =>
            OnPlayerChangeStateEmergencyCall?.Invoke(client, factionName, state);


    }

}