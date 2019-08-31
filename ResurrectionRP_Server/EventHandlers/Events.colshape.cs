using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using ResurrectionRP_Server.Utils.Extensions;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        #region Delegates
        public delegate void ColShapePlayerEventHandler(IColShape colShape, IPlayer client);
        public delegate void ColShapePlayerInteract(BsonObjectId ID, IPlayer client);
        public delegate void ColShapeVehicleEventHandler(IColShape colShape, IVehicle vehicle);
        #endregion

        #region Public events
        public static event ColShapePlayerEventHandler OnPlayerEnterColShape;
        public static event ColShapePlayerEventHandler OnPlayerLeaveColShape;
        public static event ColShapeVehicleEventHandler OnVehicleEnterColShape;
        public static event ColShapeVehicleEventHandler OnVehicleLeaveColShape;

        public static event ColShapePlayerInteract OnPlayerInteractClothingShop;
        #endregion

        #region Private methods
        private static void OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if(targetEntity.Type == BaseObjectType.Player && colShape.Exists)
                (targetEntity as IPlayer).Emit("SetStateInColShape", state);

            if (state)
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
                colShape.AddEntity(targetEntity);

                if (targetEntity.Type == BaseObjectType.Vehicle)
                    OnVehicleEnterColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    OnPlayerEnterColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
            else
            {
                // V752 : Bug ColShape.IsEntityIn() returns always false
                colShape.RemoveEntity(targetEntity as IPlayer);

                if (targetEntity.Type == BaseObjectType.Vehicle)
                    OnVehicleLeaveColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    OnPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
        }

        private static void OnEntityInteractInColShape(IPlayer client, object[] args)
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

                        OnPlayerInteractClothingShop?.Invoke(clothing, client);
                    }
                }
            }
        }
        #endregion
    }
}
