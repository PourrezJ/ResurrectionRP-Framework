using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Event = ResurrectionRP_Server.Utils.Enums.Events;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        #region Delegates
        public delegate Task ColShapePlayerEventHandler(IColShape colShape, IPlayer client);
        public delegate Task ColShapeVehicleEventHandler(IColShape colShape, IVehicle vehicle);
        #endregion

        #region Public events
        public static event ColShapePlayerEventHandler OnPlayerEnterColShape;
        public static event ColShapePlayerEventHandler OnPlayerLeaveColShape;
        public static event ColShapeVehicleEventHandler OnVehicleEnterColShape;
        public static event ColShapeVehicleEventHandler OnVehicleLeaveColShape;
        #endregion

        #region Private methods
        private static void OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if (state)
            {
                if (targetEntity.Type == BaseObjectType.Vehicle)
                    OnVehicleEnterColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    OnPlayerEnterColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
            else
            {
                if (targetEntity.Type == BaseObjectType.Vehicle)
                    OnVehicleLeaveColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    OnPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
        }
        #endregion
    }
}
