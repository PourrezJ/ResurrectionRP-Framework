using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Society
{
/*    public partial class SocietyCommands : ICommandHandler TODO
    {
        [AlternateLife.RageMP.Net.Attributes.Command("addparkingsociety")]
        public async Task AddparkingSociety(IPlayer client, string[] arguments = null)
        {
            if (!SocietyManager.AddParkingList.ContainsKey(client))
                return;

            try
            {
                Vector3 position = new Vector3();
                Vector3 rotation = new Vector3();

                if (!await client.IsInVehicleAsync())
                {
                    position = await client.GetPositionAsync();
                    rotation = await client.GetRotationAsync();
                }
                else
                {
                    var vehicle = await client.GetVehicleAsync();
                    if (!vehicle.Exists)
                        return;
                    position = await vehicle.GetPositionAsync();
                    rotation = await vehicle.GetRotationAsync();
                }

                if (SocietyManager.AddParkingList[client] != null)
                {
                    SocietyManager.AddParkingList[client].Parking = new Parking(position, new Location(position, rotation), new Location(position, rotation), limite: 50);

                    if (SocietyManager.AddParkingList.TryGetValue(client, out Society society))
                    {
                        if (society.Parking != null)
                        {
                            IColshape parkingColshape = await MP.Colshapes.NewTubeAsync(SocietyManager.AddParkingList.GetValueOrDefault(client).Parking.Spawn1.Pos, 3f, 1f);
                            // parkingColshape.SetSharedData("House_Parking", society._id);

                            await MP.Markers.NewAsync(MarkerType.VerticalCylinder, SocietyManager.AddParkingList.GetValueOrDefault(client).Parking.Spawn1.Pos - new Vector3(0.0f, 0.0f, 3f), new Vector3(), new Vector3(), 3f, Color.FromArgb(128, 255, 255, 255), true);
                            if (SocietyManager.AddParkingList.Remove(client))
                            {
                                society.InitParking(parkingColshape);
                                await society.Update();
                                await client.SendNotificationSuccess("Le parking à bien été ajouté.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MP.Logger.Error(ex.ToString(), ex);
            }
        }

        [Command("addbennys")]
        public async Task AddBennys(IPlayer client, string[] arguments)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player)
                return;

            Parking _parking = new Parking(new Vector3(-188.0847f, -1290.69f, 31.5549f), new Location(new Vector3(-188.0847f, -1290.69f, 31.5549f), new Vector3(0.1376072f, -0.009726464f, 266.3796f)));

            var benny = new Bennys("Benny's Motorsport", await client.GetPositionAsync(), 488, 40, await client.GetSocialClubNameAsync(), new Inventory(1000, 20), _parking);

            await benny.Insert();
            await benny.Load();
        }

        [Command("addunicorn")]
        public async Task AddUnicorn(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player)
                return;

            var uni = new Unicorn("Vanilla Unicorn", new Vector3(132.456f, -1290.048f, 29.26952f), 121, 73, "PapyMunjots", new Inventory(500, 40));
            await uni.Insert();
            await uni.Load();
        }

        [Command("sandjob")]
        public async Task Sandjob(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new Sandjob("SilverLake bouteilles", await client.GetPositionAsync(), 181, 28, "Xegrida", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("rhumerie")]
        public async Task Rhumerie(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new Rhumerie("Rhumerie de Mama Louise", await client.GetPositionAsync(), 93, 51, "kokochicot", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("petrolsociety")]
        public async Task PetrolSociety(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new PetrolSociety("Au Bon Réservoir", await client.GetPositionAsync(), 351, (int)BlipColor.Grey, "Spreyy_FR", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("tequilala")]
        public async Task TequilalaSociety(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new Tequilala("Tequi-La-La", await client.GetPositionAsync(), 93, (int)BlipColor.Purple, "Salem34", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("MotorShop323")]
        public async Task MotorShop323(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new WhiteWhereWolf("323 Motor Shop", await client.GetPositionAsync(), 442, (int)67, "Armex72", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("pawncar")]
        public async Task Pawncar(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player)
                return;

            Society society = new PawnCar("Pawn Car", await client.GetPositionAsync(), 267, (int)81, "Armex72", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("BSN")]
        public async Task BSN(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player)
                return;

            Society society = new BlackStreetNation("Bahama", await client.GetPositionAsync(), 121, (int)69, "Armex72", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("WildCustom")]
        public async Task WildCustom(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player)
                return;

            Society society = new WildCustom("Wild Custom", await client.GetPositionAsync(), 669, (int)5, "Armex72", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("YellowJack")]
        public async Task YellowJack(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new YellowJack("Yellow Jack", new Vector3(1982.244f, 3053.145f, 47.21507f), 93, (int)BlipColor.Yellow, "Rohakar", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }

        [Command("Amphitheatre")]
        public async Task Amphitheatre(IPlayer client)
        {
            if (PlayerManager.GetPlayerByClient(client).StaffRank <= AdminRank.Player) return;

            Society society = new YellowJack("Amphithéatre", new Vector3(688.3948f, 585.1094f, 130.4613f), 304, (int)BlipColor.Yellow, "AntoKorei", inventory: new Inventory(5000, 40));
            await society.Insert();
            await society.Load();
        }
    }*/
}
