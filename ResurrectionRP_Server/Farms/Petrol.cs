using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Farms
{
    public class Petrol : Farm
    {
        private static Vector3 RaffineriePos = new Vector3(2782.841f, 1704.716f, 23.83286f);
        private static Vector3 SellPos = new Vector3(302.9887f, -2756.656f, 5.424667f);

        private static int TrailerMaxContent = 1500;

        public static VehicleModel[] AllowedTrailers = new VehicleModel[3] { VehicleModel.ArmyTanker, VehicleModel.Tanker, VehicleModel.Tanker2 };

        public IColshape RaffinerieColshape = null;
        public IColshape SellingColshape = null;

        public ConcurrentDictionary<IPlayer, System.Timers.Timer> SellingTimer = new ConcurrentDictionary<IPlayer, System.Timers.Timer>();

        private int FillingTime = 8250;

        #region Constructor
        public Petrol()
        {
            Harvest_Name = "Puit de pétrole";
            Process_Name = "Raffinerie de pétrole";
            Selling_Name = "Exportation de pétrole";

            Harvest_BlipPosition = new Vector3(587.3458f, 2935.362f, 40.95748f);
            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_Position.Add(new Vector3(587.3458f, 2935.362f, 40.95748f));
            Process_PosRot = new Location(RaffineriePos - new Vector3(0, 0, 5), new Vector3(0f, 0f, -178f));
            Selling_PosRot = new Location(SellPos - new Vector3(0, 0, 5), new Vector3());

            Harvest_Range = 20f;

            BlipColor = Entities.Blips.BlipColor.Grey;

            ItemIDBrute = ItemID.PetrolBrute;
            ItemIDProcess = ItemID.Petrol;
            ItemPrice = 4;
            Harvest_Time = 2600; // Prod : 2600
            Process_Time = 2400; // prod 2400
            Selling_Time = 600; // prod /  600
            Enabled = true;
        }
        #endregion

        #region Events
        public override void Init()
        {
            base.Init();
            RaffinerieColshape = ColshapeManager.CreateCylinderColshape(RaffineriePos, 10, 4);
            RaffinerieColshape.OnPlayerEnterColshape += OnPlayerEnterColshape;

            SellingColshape = ColshapeManager.CreateCylinderColshape(SellPos, 10, 4);
            SellingColshape.OnPlayerEnterColshape += OnPlayerEnterColshape;

            Marker.CreateMarker(MarkerType.VerticalCylinder, RaffineriePos - new Vector3(0, 0, 1), new Vector3(10, 10, 2), Color.FromArgb(0, 0, 0));
            Marker.CreateMarker(MarkerType.VerticalCylinder, Selling_PosRot.Pos - new Vector3(0, 0, 0), new Vector3(10, 10, 2), Color.FromArgb(0, 0, 0));
            Streamer.Streamer.AddEntityTextLabel("~o~Appuyez sur E en dehors du véhicule\n pour lancer le remplissage", Harvest_Position[0], 1);
        }

        public override void OnPlayerEnterColshape(IColshape colshape, IPlayer player)
        {
            if (RaffinerieColshape.IsEntityIn(player))
            {
                VehicleHandler vehicle = null;
                List<VehicleHandler> vehs = VehiclesManager.GetNearestsVehicles(Process_PosRot.Pos, 20);

                foreach (VehicleHandler veh in vehs)
                {
                    if (veh.GetVehicleHandler().HasTrailer)
                        vehicle = (VehicleHandler)veh.Trailer;
                }

                if (vehicle != null && Array.IndexOf(AllowedTrailers, (VehicleModel)vehicle.Model) != -1)
                {
                    TankFilling(player, vehicle);
                }
            }
            else if (SellingColshape.IsEntityIn(player))
                StartSelling(player);


            base.OnPlayerEnterColshape(colshape, player);
        }
        #endregion

        #region Start Farming

        public override void StartFarming(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            VehicleHandler fuelVeh = null;

            var vehs = VehiclesManager.GetNearestsVehicles(client.Position, 5, GameMode.GlobalDimension);

            foreach(var veh in vehs)
            {
                if (veh.HasTrailer && veh.Trailer != null && Array.IndexOf(AllowedTrailers, (VehicleModel)veh.Trailer.Model) != -1)
                {
                    fuelVeh = (VehicleHandler)veh.Trailer;
                    break;
                }
            }

            if (fuelVeh == null || !fuelVeh.Exists)
            {
                client.DisplaySubtitle("~r~Aucun camion citerne à proximité", 5000);
                return;
            }
            else if (fuelVeh.HasData("Refueling"))
            {
                client.DisplaySubtitle("~r~Le camion est déjà en remplissage.", 5000);
                return;
            }
            else if (client.IsInVehicle)
            {
                client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                return;
            }

            VehicleHandler vehHandler = fuelVeh.GetVehicleHandler();

            if (vehHandler == null)
                return;
            else if (vehHandler.VehicleData.OilTank.Traite > 0)
            {
                client.DisplaySubtitle($"~r~Votre citerne contient déjà du pétrole raffiné.", 5000);
                return;
            }
            else if (vehHandler.VehicleData.OilTank.Brute >= 1500)
            {
                client.DisplaySubtitle("~r~Le camion est déjà rempli.", 5000);
                return;
            }
            else if (fuelVeh.TryGetData("Refuelling", out bool useless))
            {
                client.DisplaySubtitle("~r~Le camion est déjà en remplissage.", 5000);
                return;
            }

            client.GetPlayerHandler().IsOnProgress = true;
            Vector3 lastPos = fuelVeh.Position;
            fuelVeh.SetData("Refuelling", true);
            Item item = LoadItem.GetItemWithID(ItemIDBrute);
            client.DisplaySubtitle($"Remplissage en cours de la citerne de {item.name}", 5000);

            bool exit = false;
            int needed = 1500 - vehHandler.VehicleData.OilTank.Brute;
            client.EmitLocked("LaunchProgressBar", Harvest_Time * (needed / 10));


            FarmTimers[client] = Utils.Utils.SetInterval(async () => {

                //if (vehHandler.VehicleData.OilTank.Brute < 1500)
                //    await Task.Delay(Harvest_Time);

                if (!await client.ExistsAsync() || !await fuelVeh.ExistsAsync())
                    return;

                if (!exit && await client.IsInVehicleAsync())
                {
                    client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                    exit = true;
                }

                if (!exit && (await fuelVeh.GetPositionAsync()).Distance(lastPos) > Harvest_Range)
                {
                    client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous devez rester dans la zone.", 5000);
                    exit = true;
                }

                if (exit)
                {
                    client.StopProgressBar();
                    if (FarmTimers.ContainsKey(client))
                    {
                        FarmTimers[client].Stop();
                        FarmTimers[client].Close();
                        FarmTimers.TryRemove(client, out _);
                    }
                    return;
                }

                vehHandler.VehicleData.OilTank.Brute += 10;

                if (vehHandler.VehicleData.OilTank.Brute >= 1500)
                {
                    fuelVeh.ResetData("Refuelling");
                    client.EmitLocked("StopProgressBar");
                    client.DisplaySubtitle($"Récolte terminée: la citerne est pleine ~r~ ({vehHandler.VehicleData.OilTank.Brute}L {item.name})", 10000);
                    player.IsOnProgress = false;
                    FarmTimers[client].Stop();
                    FarmTimers[client] = null;
                    exit = true;
                }
            }, Harvest_Time);
        }
        #endregion

        #region Start Traitement
        public void TankFilling(IPlayer client, IVehicle trailer)
        {

            if (client.Vehicle != null)
                client.DisplayHelp("Vous êtes libre de sortir du véhicule le temps du traitement!", 15000);

            if (trailer == null || !trailer.Exists)
            {
                client.DisplayHelp("Aucune remorque à proximité", 5000);
                return;
            }
            else if (trailer.HasData("Refueling"))
            {
                client.DisplayHelp("La remorque est déjà en traitement.", 5000);
                return;
            }

            //if (Array.IndexOf(allowedTrailers, vehicle.Model) == -1)
            //{
            //    client.DisplayHelp("La remorque n'est pas homologuée pour cette action!", 10000);
            //    return;
            //}

            VehicleHandler vehHandler = trailer.GetVehicleHandler();

            if (vehHandler == null)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            MenuManager.CloseMenu(client);

            Item _itemNoTraite = LoadItem.GetItemWithID(ItemIDBrute);
            Item _itemTraite = LoadItem.GetItemWithID(ItemIDProcess);
            player.IsOnProgress = true;
            client.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);
            int needed = 1500 - vehHandler.VehicleData.OilTank.Brute;
            //client.LaunchProgressBar(Harvest_Time * (vehHandler.VehicleData.OilTank.Brute / 100));
            bool exit = false;
            ProcessTimers[client] = Utils.Utils.SetInterval(async () =>
            {
                if (!await client.ExistsAsync() || !await trailer.ExistsAsync())
                    return;

                if (!exit && RaffineriePos.DistanceTo(await trailer.GetPositionAsync()) > 30f)
                {
                    client.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous deviez rester dans la zone.", 5000);
                    exit = true;
                }


                if (exit)
                {
                    client.StopProgressBar();
                    if (ProcessTimers.ContainsKey(client))
                    {
                        ProcessTimers[client].Stop();
                        ProcessTimers[client].Close();
                        ProcessTimers.TryRemove(client, out _);
                    }
                    return;
                }

                if (vehHandler.VehicleData.OilTank.Traite >= TrailerMaxContent || vehHandler.VehicleData.OilTank.Brute <= 0)
                {
                    client.EmitLocked("StopProgressBar");
                    client.DisplaySubtitle($"Remplicage de la citerne terminé: Vous avez rempli ~r~ {vehHandler.VehicleData.OilTank.Traite}L {_itemTraite.name}(s)", 15000);
                    player.UpdateFull();
                    vehHandler.UpdateInBackground();
                    player.IsOnProgress = false;
                    exit = true;
                    return;
                }

                vehHandler.VehicleData.OilTank.Traite += 5;
                vehHandler.VehicleData.OilTank.Brute -= 5;
                client.DisplaySubtitle("Volume essence raffiné : " + vehHandler.VehicleData.OilTank.Traite + "L", 3000);
            }, (Process_Time * 1000) / (FillingTime));
        }
        #endregion

        #region Start Selling
        public override void StartSelling(IPlayer sender)
        {
            if (sender == null || !sender.Exists)
                return;

            PlayerHandler player = sender.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            VehicleHandler vehicle = sender.Vehicle as VehicleHandler;
            VehicleHandler trailer = null;

            if (vehicle == null || !vehicle.Exists || vehicle.Driver != sender)
                return;

            if (vehicle.Trailer != null)
            {
                trailer = (VehicleHandler)vehicle.Trailer;
            }

            if (trailer == null)
                return;

            if (Array.IndexOf(AllowedTrailers, (VehicleModel)trailer.Model) == -1)
            {
                sender.DisplaySubtitle($"Vous devez avoir un camion citerne.", 5000);
                return;
            }

            if (trailer.VehicleData.OilTank.Traite == 0)
            {
                sender.DisplaySubtitle($"Vous camion citerne est vide.", 5000);
                return;
            }
            else if (trailer.VehicleData.OilTank.Brute > 0 && trailer.VehicleData.OilTank.Traite == 0)
            {
                sender.DisplaySubtitle($"Ce n'est pas du pétrole raffiné!", 5000);
                return;
            }

            MenuManager.CloseMenu(sender);

            Item _itemBuy = LoadItem.GetItemWithID(ItemIDProcess);
            player.IsOnProgress = true;

            sender.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

            int itemcount = trailer.VehicleData.OilTank.Traite;

            var count = 0;
            bool exit = false;

            sender.LaunchProgressBar((Process_Time * 1000) / (FillingTime) * (trailer.VehicleData.OilTank.Traite / 5));
            SellingTimer[sender] = Utils.Utils.SetInterval(async () =>
            {
                if (!await sender.ExistsAsync())
                    return;          

                if (SellPos.DistanceTo(await vehicle.GetPositionAsync()) > 30f)
                {
                    sender.DisplaySubtitle($"~r~Vente interrompue: ~s~Vous devez rester dans la zone.", 5000);
                    player.IsOnProgress = false;
                    player.UpdateFull();
                    trailer.UpdateInBackground();
                    exit = true;
                }

                if (exit)
                {
                    sender.StopProgressBar();
                    if (SellingTimer.ContainsKey(sender))
                    {
                        SellingTimer[sender].Stop();
                        SellingTimer[sender].Close();
                        SellingTimer.TryRemove(sender, out _);
                    }
                    return;
                }
                else
                    sender.DisplaySubtitle($"~r~Vente en cours...", 500);

                trailer.VehicleData.OilTank.Traite -= 5;

                if (trailer.VehicleData.OilTank.Traite <= 0)
                {
                    exit = true;
                    sender.EmitLocked("StopProgressBar");
                    double gettaxe = Economy.Economy.CalculPriceTaxe(ItemPrice * itemcount, GameMode.Instance.Economy.Taxe_Exportation);
                    player.AddMoney((ItemPrice * itemcount) - gettaxe);
                    GameMode.Instance.Economy.CaissePublique += gettaxe;
                    sender.DisplaySubtitle($"~r~{itemcount} ~w~{_itemBuy.name}(s) $~r~{(ItemPrice * itemcount) - gettaxe} ~w~taxe:$~r~{gettaxe}.", 15000);
                    player.IsOnProgress = false;
                    player.UpdateFull();
                    trailer.UpdateInBackground();
                    return;
                }
            }, (Process_Time * 1000) / (FillingTime));
        }
        #endregion
    }
}