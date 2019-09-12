﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Houses
{
    public class HouseCommands
    {
        public HouseCommands()
        {
            Chat.RegisterCmd("createhouse", CreateHouse);
            Chat.RegisterCmd("addparkinghouse", Addparkinghouse);
        }

        //public async Task CreateHouse(IPlayer player, int type, int price)
        public async Task CreateHouse(IPlayer player, string[] arguments = null)
        {
            try
            {
                int type = Convert.ToInt32(arguments[0]);
                int price = Convert.ToInt32(arguments[1]);
                var position = await player.GetPositionAsync();

                if (player.GetPlayerHandler()?.StaffRank < AdminRank.Moderator)
                {
                    player.SendChatMessage("~r~ERROR: ~w~Only admins can use this command.");
                    return;
                }

                if (type < 0 || type >= HouseTypes.HouseTypeList.Count)
                {
                    player.SendChatMessage("~r~ERROR: ~w~Invalid type ID.");
                    return;
                }

                House new_house = new House(await House.GetID(), string.Empty, type, position, price, false);

                await new_house.InsertHouse();
                HouseManager.Houses.Add(new_house);
                player.SendNotificationSuccess("Maison ajouter.");
                await new_house.Load();
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
                player.SendNotificationError("Problème pour ajouter la maison");
            }
        }

        public async Task Addparkinghouse(IPlayer client, string[] arguments = null)
        {
            try
            {
                var position = await client.GetPositionAsync();
                var rotation = await client.GetRotationAsync();

                if (HouseManager.AddParkingList[client] != null)
                {
                    HouseManager.AddParkingList[client].Parking = new Parking(spawn1: new Location(position, rotation), spawn2: new Location(position, rotation), limite: HouseTypes.HouseTypeList[HouseManager.AddParkingList[client].Type].ParkingPlace);

                    if (HouseManager.AddParkingList.TryGetValue(client, out House house))
                    {
                        if (house.Parking != null)
                        {
                            IColShape parkingColshape = Alt.CreateColShapeCylinder(HouseManager.AddParkingList.GetValueOrDefault(client).Parking.Spawn1.Pos, 3f, 1f);
                            await parkingColshape.SetMetaDataAsync("House_Parking", house.ID);

                            //await MP.Markers.NewAsync(MarkerType.VerticalCylinder, HouseManager.AddParkingList.GetValueOrDefault(client).Parking.Spawn1.Pos - new Vector3(0.0f, 0.0f, 3f), new Vector3(), new Vector3(), 3f, Color.FromArgb(128, 255, 255, 255), true);
                            Marker.CreateMarker(MarkerType.VerticalCylinder, HouseManager.AddParkingList.GetValueOrDefault(client).Parking.Spawn1.Pos - new Vector3(0.0f, 0.0f, 3f), new Vector3(1,1,1));
                            if (HouseManager.AddParkingList.Remove(client))
                            {
                                house.Parking.ParkingType = ParkingType.House;
                                await house.Save();
                                client.SendNotificationSuccess("Le parking à bien été ajouté.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }
    }
}
