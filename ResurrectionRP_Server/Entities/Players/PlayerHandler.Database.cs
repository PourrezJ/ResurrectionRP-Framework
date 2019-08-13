using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using System;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        
        public async Task Save()
        {
            Alt.Server.LogDebug("Save is disabled for now, waiting to be more complete.");
            return;
            try
            {
                if (Location.Pos == Vector3.Zero)
                    return;
                await Database.MongoDB.Update(this, "players", PID);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"PlayerHandler Save { Client.SocialClubId.ToString()} {Identite?.Name ?? ""}" + ex.Data);
            }
        }

        /**public async Task UpdatePlayerInfo()
        {
            if (Client == null) return;

            try
            {
                if (!Client.Exists)
                    return;

                var vehicle = await Client.GetVehicleAsync();

                if (vehicle != null)
                {
                    if (await vehicle.GetOccupantAsync(-1) == Client)
                    {
                        var veh = vehicle.GetVehicleHandler();

                        if (veh != null)
                            await veh.Update();
                    }
                    Location = new Location(await vehicle.GetPositionAsync(), await vehicle.GetRotationAsync());
                }
                else
                {
                    Location = new Location(await Client.GetPositionAsync(), await Client.GetRotationAsync());
                }



                if (HouseManager.IsInHouse(Client))
                {
                    Location = new Location(HouseManager.GetHouse(Client).Position, new Vector3());
                }

                if (!Client.Exists)
                    return;

                Health = await Client.GetHealthAsync();

                if ((DateTime.Now - LastUpdate).Minutes >= 1)
                {
                    TimeSpent += (DateTime.Now - LastUpdate).Minutes;
                    LastUpdate = DateTime.Now;
                }

                await Save();
            }
            catch (Exception ex)
            {
                MP.Logger.Error("UpdatePlayerInfo", ex);
            }
        } **/
    }
}
