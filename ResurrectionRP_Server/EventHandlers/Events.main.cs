using AltV.Net.Async;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        public static void Initialize()
        {
            AltAsync.OnColShape += OnEntityColshape;
            AltAsync.OnClient("InteractionInColshape", OnEntityInteractInColShape);
        }
    }
}
