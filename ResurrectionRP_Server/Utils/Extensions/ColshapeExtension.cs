using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.EventHandlers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResurrectionRP_Server
{
    // BUG v784 : ColShape.IsEntityIn() returns always false
    public static class ColshapeExtension
    {
        public static void AddEntity(this IColShape colshape, IEntity entity)
        {
            if (!entity.Exists)
                return;

            lock (colshape)
            {
                colshape.GetData("Entities", out List<IEntity> entities);

                if (entities != null && !entities.Contains(entity))
                    entities.Add(entity);
                else if (entities == null)
                {
                    entities = new List<IEntity>();
                    entities.Add(entity);
                    colshape.SetData("Entities", entities);
                }
            }
        }

        public static void RemoveEntity(this IColShape colshape, IEntity entity)
        {
            lock (colshape)
            {
                colshape.GetData("Entities", out List<IEntity> entities);

                if (entities != null && entities.Contains(entity))
                    entities.Remove(entity);
            }
        }

        public static bool IsEntityInColShape(this IColShape colshape, IEntity client)
        {
            lock (colshape)
            {
                colshape.GetData("Entities", out List<IEntity> entities);

                if (entities == null || !entities.Contains(client))
                    return false;
            }

                return true;
        }
        
        public static void SetOnPlayerEnterColShape(this IColShape colshape, Events.ColShapePlayerEventHandler method)
        {
            lock (colshape)
            {
                colshape.GetData("OnPlayerEnterColShape", out Events.ColShapePlayerEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = method;
                else
                {
                    eventHandler = method;
                    colshape.SetData("OnPlayerEnterColShape", eventHandler);
                }
            }
        }

        public static void UnsetOnPlayerEnterColShape(this IColShape colshape)
        {
            lock (colshape)
            {
                colshape.GetData("OnPlayerEnterColShape", out Events.ColShapePlayerEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = null;
            }
        }

        public static void SetOnPlayerLeaveColShape(this IColShape colshape, Events.ColShapePlayerEventHandler method)
        {
            lock (colshape)
            {
                colshape.GetData("OnPlayerLeaveColShape", out Events.ColShapePlayerEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = method;
                else
                {
                    eventHandler = method;
                    colshape.SetData("OnPlayerLeaveColShape", eventHandler);
                }
            }
        }

        public static void UnsetOnPlayerLeaveColShape(this IColShape colshape)
        {
            lock (colshape)
            {
                colshape.GetData("OnPlayerLeaveColShape", out Events.ColShapePlayerEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = null;
            }
        }

        public static void SetOnVehicleEnterColShape(this IColShape colshape, Events.ColShapeVehicleEventHandler method)
        {
            lock (colshape)
            {
                colshape.GetData("OnVehicleEnterColShape", out Events.ColShapeVehicleEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = method;
                else
                {
                    eventHandler = method;
                    colshape.SetData("OnVehicleEnterColShape", eventHandler);
                }
            }
        }

        public static void UnsetOnVehicleEnterColShape(this IColShape colshape)
        {
            lock (colshape)
            {
                colshape.GetData("OnVehicleEnterColShape", out Events.ColShapeVehicleEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = null;
            }
        }

        public static void SetOnVehicleLeaveColShape(this IColShape colshape, Events.ColShapeVehicleEventHandler method)
        {
            lock (colshape)
            {
                colshape.GetData("OnVehicleLeaveColShape", out Events.ColShapeVehicleEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = method;
                else
                {
                    eventHandler = method;
                    colshape.SetData("OnVehicleLeaveColShape", eventHandler);
                }
            }
        }

        public static void UnsetOnVehicleLeaveColShape(this IColShape colshape)
        {
            lock (colshape)
            {
                colshape.GetData("OnVehicleLeaveColShape", out Events.ColShapeVehicleEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = null;
            }
        }

        public static void SetOnPlayerInteractInColShape(this IColShape colshape, Events.ColShapePlayerEventHandlerAsync method)
        {
            lock (colshape)
            {
                colshape.GetData("OnPlayerInteractInColShape", out Events.ColShapePlayerEventHandlerAsync eventHandler);

                if (eventHandler != null)
                    eventHandler = method;
                else
                {
                    eventHandler = method;
                    colshape.SetData("OnPlayerInteractInColShape", eventHandler);
                }
            }
        }

        public static void UnsetOnPlayerInteractInColShape(this IColShape colshape)
        {
            lock (colshape)
            {
                colshape.GetData("OnPlayerInteractInColShape", out Events.ColShapePlayerEventHandler eventHandler);

                if (eventHandler != null)
                    eventHandler = null;
            }
        }
    }
}
