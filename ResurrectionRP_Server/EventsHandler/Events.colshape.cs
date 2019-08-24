using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Event = ResurrectionRP_Server.Utils.Enums.Events;

namespace ResurrectionRP_Server.EventsHandler
{

    public delegate Task OnPlayerColshape(IColShape colShape, IPlayer client);
    public delegate Task OnVehicleColshape(IColShape colShape, IVehicle vehicle);

    public partial class Events
    {
        public static void OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if (state)
            {
                if (targetEntity.Type == BaseObjectType.Vehicle)
                    Alt.Emit(Event.OnVehicleEnterColShape, colShape, targetEntity as IVehicle, state);
                else if (targetEntity.Type == BaseObjectType.Player)
                    Alt.Emit(Event.OnPlayerEnterColShape, colShape, targetEntity as IPlayer, state);
            }
            else
            {
                if (targetEntity.Type == BaseObjectType.Vehicle)
                    Alt.Emit(Event.OnVehicleExitColShape, colShape, targetEntity as IVehicle, state);
                else if (targetEntity.Type == BaseObjectType.Player)
                    Alt.Emit(Event.OnPlayerExitColShape, colShape, targetEntity as IPlayer, state);
            }


        }
    }
}
