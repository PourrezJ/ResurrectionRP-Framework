using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        #region Delegates
        public delegate Task ColShapePlayerEventHandler(IColShape colShape, IPlayer client);
        public delegate Task ColShapePlayerInteract(BsonObjectId ID, IPlayer client);
        public delegate Task ColShapeVehicleEventHandler(IColShape colShape, IVehicle vehicle);
        #endregion

        #region Public events
        public static event ColShapePlayerEventHandler OnPlayerEnterColShape;
        public static event ColShapePlayerEventHandler OnPlayerLeaveColShape;
        public static event ColShapeVehicleEventHandler OnVehicleEnterColShape;
        public static event ColShapeVehicleEventHandler OnVehicleLeaveColShape;

        public static event ColShapePlayerInteract OnPlayerInteractClothingShop;
        public static event ColShapePlayerEventHandler OnPlayerInteractHouse;
        public static event ColShapePlayerEventHandler OnPlayerInteractTeleporter;
        #endregion

        #region Private methods
        private static async Task OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if(targetEntity.Type == BaseObjectType.Player && colShape.Exists)
                (targetEntity as IPlayer).EmitLocked("SetStateInColShape", state);

            if (state)
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
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
                // V752 : Bug ColShape.IsEntityIn() returns always false
                colShape.RemoveEntity(targetEntity as IPlayer);

                if (targetEntity.Type == BaseObjectType.Vehicle)
                {
                    if (OnVehicleLeaveColShape != null)
                        await OnVehicleLeaveColShape.Invoke(colShape, (IVehicle)targetEntity);

                    if(colShape.GetData("OnVehicleLeaveColShape", out ColShapeVehicleEventHandler onVehicleLeaveColShape))
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

        private static async Task OnEntityInteractInColShape(IPlayer client, object[] args)
        {
            if (!int.TryParse(args[0].ToString(), out int key))
                return;

            foreach (IColShape colshape in Alt.GetAllColShapes())
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
                if (colshape.IsEntityInColShape(client))
                {
                    if (colshape.GetData("ClothingID", out BsonObjectId clothing) && clothing != null)
                    {
                        if (key != 69)
                            return;

                        if (OnPlayerInteractClothingShop != null)
                            await OnPlayerInteractClothingShop.Invoke(clothing, client);
                    }
                    else if (colshape.GetData("Teleport", out string TeleportID) && TeleportID != null)
                    {
                        if (key != 69)
                            return;

                        if (OnPlayerInteractTeleporter != null)
                            await OnPlayerInteractTeleporter.Invoke(colshape, client);
                    }
                    else if (colshape.GetData("House", out string HouseID) && HouseID != null)
                    {
                        if (key != 69)
                            return;

                        if (OnPlayerInteractHouse != null)
                            await OnPlayerInteractHouse.Invoke(colshape, client);
                    }
                }
            }
        }
        #endregion
    }
}
