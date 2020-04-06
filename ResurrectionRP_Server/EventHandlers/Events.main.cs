using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.EventHandlers
{

    public static partial class Events
    {
        #region Delegates
        public delegate void FactionPriseService(IPlayer client, string FactionName);
        public delegate void FactionOutPriseService(IPlayer client, string FactionName);
        #endregion

        #region Public events
        public static event FactionPriseService OnFactionPlayerTakeService;
        public static event FactionOutPriseService OnFactionPlayerOutService;
        #endregion

        public static void Initialize()
        {
            Alt.OnClient<IPlayer, string, string, string>("InteractEmergencyCall", InteractEmergencyCall);
        }
    }
}
