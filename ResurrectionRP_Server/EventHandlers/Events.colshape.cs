using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Event = ResurrectionRP_Server.Utils.Enums.Events;
using MongoDB.Bson;

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
                colShape.putPlayerInColshape((targetEntity as IPlayer));
                if (targetEntity.Type == BaseObjectType.Vehicle)
                    OnVehicleEnterColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    OnPlayerEnterColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
            else
            {
                colShape.RemovePlayerInColshape((targetEntity as IPlayer));
                if (targetEntity.Type == BaseObjectType.Vehicle)
                    OnVehicleLeaveColShape?.Invoke(colShape, (IVehicle)targetEntity);
                else if (targetEntity.Type == BaseObjectType.Player)
                    OnPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);
            }
        }

        private static async Task OnEntityInteractInColShape(IPlayer client, object[] args)
        {
            int key = int.Parse(args[0] + "");
            foreach (IColShape colshape in Alt.GetAllColShapes())
            {
                if(await colshape.IsPlayerInColshape(client))
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
