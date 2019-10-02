using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Collections.Generic;
using System;
using System.Numerics;
using System.Threading;

namespace ResurrectionRP_Server.Colshape
{
    #region Delegates
    public delegate void ColshapePlayerEventHandler(IColshape colshape, IPlayer client);
    public delegate void ColshapeVehicleEventHandler(IColshape colshape, IVehicle vehicle);
    #endregion

    public static class ColshapeManager
    {
        #region Constants
        private const int TIME_INTERVAL = 50;
        #endregion

        #region Private static fields
        private static Thread _thread = null;
        private static bool _running = true;
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
            if (_thread != null)
                return;

            _thread = new Thread(OnTick);
            _thread.IsBackground = true;
            _thread.Start();
        }
        #endregion

        #region Private static methods
        public static void OnTick()
        {
            if (!_running)
                return;

            try
            {
                DateTime startTime = DateTime.Now;

                foreach (IPlayer player in Alt.GetAllPlayers())
                {
                    lock (_colshapes)
                    {
                        foreach (IColshape colshape in _colshapes.Values)
                        {
                            if (!colshape.IsEntityIn(player) && colshape.IsEntityInside(player))
                            {
                                colshape.AddEntity(player);
                                OnPlayerEnterColshape?.Invoke(colshape, player);
                                player.EmitLocked("OnPlayerEnterColshape", colshape.Id);
                                Alt.Log($"[Colshape {colshape.Id}] Player {player.Id} entering, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                            }
                        }
                    }
                }

                foreach (IVehicle vehicle in VehiclesManager.GetAllVehiclesInGame())
                {
                    lock (_colshapes)
                    {
                        foreach (IColshape colshape in _colshapes.Values)
                        {
                            if (!colshape.IsEntityIn(vehicle) && colshape.IsEntityInside(vehicle))
                            {
                                Alt.Log($"[Colshape {colshape.Id}] Vehicle {vehicle.Id} entering");
                                colshape.AddEntity(vehicle);
                                OnVehicleEnterColshape?.Invoke(colshape, vehicle);
                            }
                        }
                    }
                }

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
                                        Alt.Log($"[Colshape {colshape.Id}] Player {entity.Id} leaving, {(DateTime.Now - startTime).TotalMilliseconds}ms");
                                    }
                                    else if (entity.Type == BaseObjectType.Vehicle)
                                    {
                                        OnVehicleLeaveColshape?.Invoke(colshape, (IVehicle)entity);
                                        Alt.Log($"[Colshape {colshape.Id}] Vehicle {entity.Id} leaving, {(DateTime.Now - startTime).TotalMilliseconds}ms");
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

                Thread.Sleep(Math.Max(TIME_INTERVAL - (int)(DateTime.Now - startTime).TotalMilliseconds, 0));
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"Colshape error {ex}");
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

        public static void Shutdown()
        {
            _running = false;
        }
        #endregion
    }
}
