using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using Newtonsoft.Json;
using ResurrectionRP_Server.Houses;
using System;
using System.Threading.Tasks;

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
        public static event ColShapePlayerEventHandler OnPlayerInteractInColShape;
        #endregion

        #region Private methods
        private static async Task OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            try
            {
                if (targetEntity.Type == BaseObjectType.Player && colShape.Exists)
                    (targetEntity as IPlayer).EmitLocked("SetStateInColShape", state);

                if (state)
                {
                    // BUG V784 : Bug ColShape.IsEntityIn() returns always false
                    colShape.AddEntity(targetEntity);

                    if (targetEntity.Type == BaseObjectType.Vehicle)
                    {
                        if (OnVehicleEnterColShape != null)
                            await OnVehicleEnterColShape.Invoke(colShape, (IVehicle)targetEntity);

                        if (colShape.GetData("OnVehicleEnterColShape", out ColShapeVehicleEventHandler onVehicleEnterColShape))
                            await onVehicleEnterColShape.Invoke(colShape, (IVehicle)targetEntity);
                    }
                    else if (targetEntity.Type == BaseObjectType.Player)
                    {
                        if (OnPlayerEnterColShape != null)
                            await OnPlayerEnterColShape.Invoke(colShape, (IPlayer)targetEntity);

                        if (colShape.GetData("OnPlayerEnterColShape", out ColShapePlayerEventHandler onPlayerEnterColShape))
                            await onPlayerEnterColShape.Invoke(colShape, (IPlayer)targetEntity);
                    }
                }
                else
                {
                    // BUG V784 : Bug ColShape.IsEntityIn() returns always false
                    colShape.RemoveEntity(targetEntity as IPlayer);

                    if (targetEntity.Type == BaseObjectType.Vehicle)
                    {
                        if (OnVehicleLeaveColShape != null)
                            await OnVehicleLeaveColShape.Invoke(colShape, (IVehicle)targetEntity);

                        if (colShape.GetData("OnVehicleLeaveColShape", out ColShapeVehicleEventHandler onVehicleLeaveColShape))
                            await onVehicleLeaveColShape.Invoke(colShape, (IVehicle)targetEntity);
                    }
                    else if (targetEntity.Type == BaseObjectType.Player)
                    {
                        if (OnPlayerLeaveColShape != null)
                            await OnPlayerLeaveColShape.Invoke(colShape, (IPlayer)targetEntity);

                        if (colShape.GetData("OnPlayerLeaveColShape", out ColShapePlayerEventHandler onPlayerLeaveColShape))
                            await onPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);
                    }
                }
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        private static async Task OnEntityInteractInColShape(IPlayer client, object[] args)
        {
            if (!int.TryParse(args[0].ToString(), out int key))
                return;

            if (key != 69)
                return;

            foreach (IColShape colshape in Alt.GetAllColShapes())
            {
                // BUG V784 : Bug ColShape.IsEntityIn() returns always false
                if (colshape.IsEntityInColShape(client))
                {
                    if (OnPlayerInteractInColShape != null)
                        await OnPlayerInteractInColShape.Invoke(colshape, client);

                    if (colshape.GetData("OnPlayerInteractInColShape", out ColShapePlayerEventHandler onPlayerInteractInColShape))
                        await onPlayerInteractInColShape.Invoke(colshape, client);

                    break;
                }
            }
        }
        #endregion
    }
}
