using AltV.Net;
using AltV.Net.Async;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        public static void Initialize()
        {
            Alt.OnColShape += OnEntityColshape;
            Alt.OnClient("InteractionInColshape", OnEntityInteractInColShape);
        }
    }
}
