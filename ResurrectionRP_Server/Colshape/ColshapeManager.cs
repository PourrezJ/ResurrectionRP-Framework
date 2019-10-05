﻿using AltV.Net;
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
        private static readonly Dictionary<ulong, IColshape> _colshapes = new Dictionary<ulong, IColshape>();
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
        }
        #endregion

        #region Private static methods
        private static void OnPlayerDisconnect(IPlayer player, string reason)
        {
            DateTime startTime = DateTime.Now;

            try
            {
                lock (_colshapes)
                {
                    foreach (IColshape colshape in _colshapes.Values)
                    {
                        if (colshape.IsEntityIn(player))
                        {
                            colshape.RemoveEntity(player);
                            OnPlayerLeaveColshape?.Invoke(colshape, player);
                            Alt.Log($"[Colshape {colshape.Id}] Player {player.Id} leaving, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
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
                            try
                            {
                                if (!colshape.IsEntityInside(entity))
                                {
                                    _entitiesToRemove.Add(entity);

                                    if (entity.Type == BaseObjectType.Player)
                                    {
                                        OnPlayerLeaveColshape?.Invoke(colshape, (IPlayer)entity);
                                        ((IPlayer)entity).EmitLocked("OnPlayerLeaveColshape", colshape.Id);
                                        Alt.Log($"[Colshape {colshape.Id}] Player {entity.Id} leaving, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                                    }
                                    else if (entity.Type == BaseObjectType.Vehicle)
                                    {
                                        OnVehicleLeaveColshape?.Invoke(colshape, (IVehicle)entity);
                                        Alt.Log($"[Colshape {colshape.Id}] Vehicle {entity.Id} leaving, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Alt.Server.LogError(ex.ToString());
                            }
                        }

                        if (_entitiesToRemove.Count > 0)
                        {
                            try
                            {
                                foreach (IEntity entity in _entitiesToRemove)
                                    colshape.RemoveEntity(entity);

                                _entitiesToRemove.Clear();
                            }
                            catch (Exception ex)
                            {
                                Alt.Server.LogError(ex.ToString());
                            }
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
                        try
                        {
                            if (!colshape.IsEntityIn(player) && colshape.IsEntityInside(player))
                            {
                                colshape.AddEntity(player);
                                OnPlayerEnterColshape?.Invoke(colshape, player);
                                player.EmitLocked("OnPlayerEnterColshape", colshape.Id);
                                Alt.Log($"[Colshape {colshape.Id}] Player {player.Id} entering, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                            }
                        }
                        catch (Exception ex)
                        {
                            Alt.Server.LogError(ex.ToString());
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
                        try
                        {
                            if (!colshape.IsEntityIn(vehicle) && colshape.IsEntityInside(vehicle))
                            {
                                Alt.Log($"[Colshape {colshape.Id}] Vehicle {vehicle.Id} entering, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                                colshape.AddEntity(vehicle);
                                OnVehicleEnterColshape?.Invoke(colshape, vehicle);
                            }
                        }
                        catch (Exception ex)
                        {
                            Alt.Server.LogError(ex.ToString());
                        }
                    }
                }
            }
        }
        #endregion
    }
}
