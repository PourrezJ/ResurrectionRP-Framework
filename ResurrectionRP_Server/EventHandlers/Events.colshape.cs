using AltV.Net;
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
        public static event ColShapePlayerEventHandler OnPlayerInteractTeleporter;
        #endregion

        #region Private methods
        private static async Task OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if(targetEntity.Type == BaseObjectType.Player && colShape.Exists)
                (targetEntity as IPlayer).Emit("SetStateInColShape", state);

            if (state)
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
                colShape.AddEntity(targetEntity);

                if (targetEntity.Type == BaseObjectType.Vehicle)
                    await OnVehicleEnterColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    await OnPlayerEnterColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
            else
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
                colShape.RemoveEntity(targetEntity as IPlayer);

                if (targetEntity.Type == BaseObjectType.Vehicle)
                    await OnVehicleLeaveColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    await OnPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
        }

        private static async Task OnEntityInteractInColShape(IPlayer client, object[] args)
        {
            int key = int.Parse(args[0].ToString());

            foreach (IColShape colshape in Alt.GetAllColShapes())
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
                if (colshape.IsEntityInColShape(client))
                {
                    if (colshape.GetData("ClothingID", out BsonObjectId clothing) && clothing != null)
                    {
                        if (key != 69)
                            return;
                        await OnPlayerInteractClothingShop?.Invoke(clothing, client);
                    } else if (colshape.GetData("Teleport", out string TeleportID) && TeleportID != null)
                    {
                        if (key != 69)
                            return;
                        await OnPlayerInteractTeleporter?.Invoke(colshape, client);
                    }
                }
            }
        }
        #endregion
    }
}
