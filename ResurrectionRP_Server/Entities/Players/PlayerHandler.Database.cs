﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Models;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        #region Fields
        private DateTime _lastUpdateRequest;
        private bool _updateWaiting = false;
        private int _nbUpdateRequests;
        #endregion

        #region Methods
        public void UpdateFull()
        {
            AltAsync.Do(() =>
            {
                if (Client == null || !Client.Exists)
                    return;

                try
                {
                    VehicleHandler veh = null;

                    Health = Client.Health;
                    IVehicle vehicle = Client.Vehicle;

                    if (vehicle != null && vehicle.Exists)
                    {
                        if (vehicle.Driver == Client)
                            veh = vehicle.GetVehicleHandler();

                        Location = new Location(vehicle.Position, vehicle.Rotation);
                    }
                    else if (HouseManager.IsInHouse(Client))
                        Location = new Location(HouseManager.GetHouse(Client).Position, new Vector3());
                    else
                        Location = new Location(Client.Position, Client.Rotation);

                    if (veh != null)
                        veh.UpdateInBackground();

                    if ((DateTime.Now - LastUpdate).Minutes >= 1)
                    {
                        TimeSpent += (DateTime.Now - LastUpdate).Minutes;
                        LastUpdate = DateTime.Now;
                    }

                    UpdateInBackground();
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"PlayerHandler.Update() - {Client.GetSocialClub()}, {Identite.Name} - {ex}");
                }
            }).Wait();
        }

        public void UpdateInBackground()
        {
            _lastUpdateRequest = DateTime.Now;

            if (_updateWaiting)
            {
                _nbUpdateRequests++;
                return;
            }

            _updateWaiting = true;
            _nbUpdateRequests = 1;

            Task.Run(async () =>
            {
                DateTime updateTime = _lastUpdateRequest.AddMilliseconds(Globals.SAVE_WAIT_TIME);

                while (DateTime.Now < updateTime)
                {
                    TimeSpan waitTime = updateTime - DateTime.Now;

                    if (waitTime.TotalMilliseconds < 1)
                        waitTime = new TimeSpan(0, 0, 0, 0, 1);

                    await Task.Delay((int)waitTime.TotalMilliseconds);
                    updateTime = _lastUpdateRequest.AddMilliseconds(Globals.SAVE_WAIT_TIME);
                }

                try
                {
                    if (Location.Pos == Vector3.Zero)
                        return;

                    var result = await Database.MongoDB.Update(this, "players", PID, _nbUpdateRequests);

                    if (result.MatchedCount == 0)
                        Alt.Server.LogWarning($"Update error for player {Client.GetSocialClub()} - {Identite.Name}");

                    _updateWaiting = false;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"PlayerHandler.UpdateInBackground() - {Client.GetSocialClub()}, {Identite.Name} - {ex}");
                }
            });
        }
        #endregion
    }
}
