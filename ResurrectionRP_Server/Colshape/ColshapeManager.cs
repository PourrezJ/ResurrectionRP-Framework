using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Collections.Generic;
using System;
using System.Numerics;

namespace ResurrectionRP_Server.Colshape
{
    #region Delegates
    public delegate void ColshapePlayerEventHandler(IColshape colshape, IPlayer client);
    public delegate void ColshapeVehicleEventHandler(IColshape colshape, IVehicle vehicle);
    #endregion

    public static class ColshapeManager
    {
        #region Private static fields
        private static volatile uint _colshapeId = 0;
        private static readonly Dictionary<long, IColshape> _colshapes = new Dictionary<long, IColshape>();
        private static readonly HashSet<IEntity> _entitiesToRemove = new HashSet<IEntity>();
        #endregion

        #region Events
        public static event ColshapePlayerEventHandler OnPlayerEnterColshape;
        public static event ColshapePlayerEventHandler OnPlayerLeaveColshape;
        public static event ColshapePlayerEventHandler OnPlayerInteractInColshape;
        public static event ColshapeVehicleEventHandler OnVehicleEnterColshape;
        public static event ColshapeVehicleEventHandler OnVehicleLeaveColshape;
        #endregion

        #region Init
        public static void Init()
        {
            Alt.OnPlayerDisconnect += OnPlayerDisconnect;
            Alt.OnVehicleRemove += OnVehicleRemove;
            Alt.OnClient("InteractionInColshape", OnEntityInteractInColShape);
        }
        #endregion

        #region Event handlers
        private static void OnPlayerDisconnect(IPlayer player, string reason)
        {
            DateTime startTime = DateTime.Now;

            lock (_colshapes)
            {
                foreach (IColshape colshape in _colshapes.Values)
                {
                    if (colshape.IsEntityIn(player))
                    {
                        colshape.RemoveEntity(player);
                        OnPlayerLeaveColshape?.Invoke(colshape, player);
                        Alt.Log($"[Colshape {colshape.Id}] Player {player.Id} removed, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count}");
                    }
                }
            }
        }

        private static void OnVehicleRemove(IVehicle vehicle)
        {
            DateTime startTime = DateTime.Now;

            lock (_colshapes)
            {
                foreach (IColshape colshape in _colshapes.Values)
                {
                    if (colshape.IsEntityIn(vehicle))
                    {
                        colshape.RemoveEntity(vehicle);
                        OnVehicleLeaveColshape?.Invoke(colshape, vehicle);
                        Alt.Log($"[Colshape {colshape.Id}] Vehicle {vehicle.Id} removed, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count}");
                    }
                }
            }
        }
        #endregion

        #region Public static methods
        public static IColshape CreateCylinderColshape(Vector3 position, float radius, float height, short dimension = GameMode.GlobalDimension)
        {
            IColshape colshape = new CylinderColshape(_colshapeId++, position, radius, height, dimension);

            lock (_colshapes)
            {
                _colshapes.Add(colshape.Id, colshape);
            }

            return colshape;
        }

        public static IColshape CreateSphereColshape(Vector3 position, float radius, short dimension = GameMode.GlobalDimension)
        {
            IColshape colshape = new SphereColshape(_colshapeId++, position, radius, dimension);

            lock (_colshapes)
            {
                _colshapes.Add(colshape.Id, colshape);
            }

            return colshape;
        }

        public static void DeleteColshape(IColshape colshape)
        {
            lock (_colshapes)
            {
                _colshapes.Remove(colshape.Id, out _);
            }
        }

        public static void OnTick()
        {
            DateTime startTime = DateTime.Now;

            lock (_colshapes)
            {
                foreach (IColshape colshape in _colshapes.Values)
                {
                    lock (colshape.Entities)
                    {
                        foreach (IEntity entity in colshape.Entities)
                        {
                            if (!colshape.IsEntityInside(entity))
                            {
                                _entitiesToRemove.Add(entity);

                                if (entity.Type == BaseObjectType.Player)
                                {
                                    OnPlayerLeaveColshape?.Invoke(colshape, (IPlayer)entity);
                                    ((IPlayer)entity).EmitLocked("OnPlayerLeaveColshape", colshape.Id);
                                    Alt.Log($"[Colshape {colshape.Id}] Player {entity.Id} leaving, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count - _entitiesToRemove.Count}");
                                }
                                else if (entity.Type == BaseObjectType.Vehicle)
                                {
                                    OnVehicleLeaveColshape?.Invoke(colshape, (IVehicle)entity);
                                    Alt.Log($"[Colshape {colshape.Id}] Vehicle {entity.Id} leaving, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count - _entitiesToRemove.Count}");
                                }
                            }
                        }

                        if (_entitiesToRemove.Count > 0)
                        {
                            foreach (IEntity entity in _entitiesToRemove)
                                colshape.RemoveEntity(entity);

                            _entitiesToRemove.Clear();
                        }
                    }
                }
            }

            foreach (IPlayer player in Alt.GetAllPlayers())
            {
                if (!player.Exists)
                    continue;

                lock (_colshapes)
                {
                    foreach (IColshape colshape in _colshapes.Values)
                    {
                        if (!colshape.IsEntityIn(player) && colshape.IsEntityInside(player))
                        {
                            colshape.AddEntity(player);
                            OnPlayerEnterColshape?.Invoke(colshape, player);
                            player.EmitLocked("OnPlayerEnterColshape", colshape.Id);
                            Alt.Log($"[Colshape {colshape.Id}] Player {player.Id} entering, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count}");
                        }
                    }
                }
            }

            foreach (IVehicle vehicle in VehiclesManager.GetAllVehiclesInGame())
            {
                if (!vehicle.Exists)
                    continue;

                lock (_colshapes)
                {
                    foreach (IColshape colshape in _colshapes.Values)
                    {
                        if (!colshape.IsEntityIn(vehicle) && colshape.IsEntityInside(vehicle))
                        {
                            colshape.AddEntity(vehicle);
                            OnVehicleEnterColshape?.Invoke(colshape, vehicle);
                            Alt.Log($"[Colshape {colshape.Id}] Vehicle {vehicle.Id} entering, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count}");
                        }
                    }
                }
            }
        }

        private static void OnEntityInteractInColShape(IPlayer client, object[] args)
        {
            try
            {
                DateTime startTime = DateTime.Now;

                if (!int.TryParse(Convert.ToString(args[0]), out int key) || key != 69)
                    return;

                lock (_colshapes)
                {
                    IColshape colshape = _colshapes[(long)args[1]];

                    if (colshape.IsEntityIn(client))
                    {
                        colshape.PlayerInteractInColshape(client);
                        OnPlayerInteractInColshape?.Invoke(colshape, client);
                        Alt.Log($"[Colshape {colshape.Id}] Player {client.Id} interacting, {Math.Round((DateTime.Now - startTime).TotalMilliseconds, 4)}ms, Entities: {colshape.Entities.Count}");
                    }
                }
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }
        #endregion
    }
}
