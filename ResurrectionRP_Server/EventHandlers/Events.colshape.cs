using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.EventHandlers
{
    public static partial class Events
    {
        #region Delegates
        public delegate void ColShapePlayerEventHandler(IColShape colShape, IPlayer client);
        public delegate Task ColShapePlayerEventHandlerAsync(IColShape colShape, IPlayer client);
        public delegate void ColShapeVehicleEventHandler(IColShape colShape, IVehicle vehicle);
        #endregion

        #region Public events
        public static event ColShapePlayerEventHandler OnPlayerEnterColShape;
        public static event ColShapePlayerEventHandler OnPlayerLeaveColShape;
        public static event ColShapeVehicleEventHandler OnVehicleEnterColShape;
        public static event ColShapeVehicleEventHandler OnVehicleLeaveColShape;
        public static event ColShapePlayerEventHandlerAsync OnPlayerInteractInColShapeAsync;
        public static event ColShapePlayerEventHandler OnPlayerInteractInColShape;
        #endregion

        #region Private fields
        private static ConcurrentDictionary<IEntity, IColShape> _playerColshape = new ConcurrentDictionary<IEntity, IColShape>();
        #endregion

        #region Private methods
        private static void OnEntityColshape(IColShape colShape, IEntity targetEntity, bool state)
        {
            try
            {
                if (state)
                {
                    // BUG v792 : Teleport to colshape faraway doesn't always fire OnColShape() event
                    IColShape oldColShape = _playerColshape.GetOrAdd(targetEntity, colShape);

                    if (oldColShape != colShape)
                        OnEntityColshape(_playerColshape[targetEntity], targetEntity, false);

                    // Alt.Server.LogInfo($"Entity {targetEntity.Id} enter colshape {colShape.NativePointer.ToString()}");
                    // BUG V784 : Bug ColShape.IsEntityIn() returns always false
                    colShape.AddEntity(targetEntity);

                    if (targetEntity.Type == BaseObjectType.Vehicle)
                    {
                        OnVehicleEnterColShape?.Invoke(colShape, (IVehicle)targetEntity);

                        if (colShape.GetData("OnVehicleEnterColShape", out ColShapeVehicleEventHandler onVehicleEnterColShape))
                            onVehicleEnterColShape?.Invoke(colShape, (IVehicle)targetEntity);
                    }
                    else if (targetEntity.Type == BaseObjectType.Player)
                    {
                        OnPlayerEnterColShape?.Invoke(colShape, (IPlayer)targetEntity);

                        if (colShape.GetData("OnPlayerEnterColShape", out ColShapePlayerEventHandler onPlayerEnterColShape))
                            onPlayerEnterColShape?.Invoke(colShape, (IPlayer)targetEntity);
                    }
                }
                else
                {
                    // BUG v792 : Teleport to colshape faraway doesn't always fire OnColShape() event
                    _playerColshape.TryRemove(targetEntity, out _);

                    // Alt.Server.LogInfo($"Entity {targetEntity.Id} leave colshape {colShape.NativePointer.ToString()}");
                    // BUG V784 : Bug ColShape.IsEntityIn() returns always false
                    colShape.RemoveEntity(targetEntity);

                    if (targetEntity.Type == BaseObjectType.Vehicle)
                    {
                        OnVehicleLeaveColShape?.Invoke(colShape, (IVehicle)targetEntity);

                        if (colShape.GetData("OnVehicleLeaveColShape", out ColShapeVehicleEventHandler onVehicleLeaveColShape))
                            onVehicleLeaveColShape?.Invoke(colShape, (IVehicle)targetEntity);
                    }
                    else if (targetEntity.Type == BaseObjectType.Player)
                    {
                        OnPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);

                        if (colShape.GetData("OnPlayerLeaveColShape", out ColShapePlayerEventHandler onPlayerLeaveColShape))
                            onPlayerLeaveColShape?.Invoke(colShape, (IPlayer)targetEntity);
                    }
                }

                if (targetEntity.Type == BaseObjectType.Player && targetEntity.Exists)
                    ((IPlayer)targetEntity).EmitLocked("SetStateInColShape", state);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        private static void OnEntityInteractInColShape(IPlayer client, object[] args)
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
                    if (OnPlayerInteractInColShapeAsync != null)
                        Task.Run(()=> OnPlayerInteractInColShapeAsync.Invoke(colshape, client));

                    if (OnPlayerInteractInColShape != null)
                        OnPlayerInteractInColShape.Invoke(colshape, client);

                    if (colshape.GetData("OnPlayerInteractInColShapeAsync", out ColShapePlayerEventHandlerAsync onPlayerInteractInColShapeAsync) && onPlayerInteractInColShapeAsync != null)
                        Task.Run(() => onPlayerInteractInColShapeAsync.Invoke(colshape, client));

                    if (colshape.GetData("OnPlayerInteractInColShape", out ColShapePlayerEventHandler onPlayerInteractInColShape) && onPlayerInteractInColShape != null)
                        onPlayerInteractInColShape.Invoke(colshape, client);
                    break;
                }
            }
        }
        #endregion
    }
}
