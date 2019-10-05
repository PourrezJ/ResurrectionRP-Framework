using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
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

        public static VehicleModel[] allowedTrailers = new VehicleModel[3] { VehicleModel.ArmyTanker, VehicleModel.Tanker, VehicleModel.Tanker2 };

        public IColshape RaffinerieColshape = null;
        public IColshape SellingColshape = null;

        public ConcurrentDictionary<IPlayer, System.Timers.Timer> Timers = new ConcurrentDictionary<IPlayer, System.Timers.Timer>();

        private int FillingTime = 8250;

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
            Process_PosRot = new Location(RaffineriePos - new Vector3(0,0,5), new Vector3(0f, 0f, -178f));
            Selling_PosRot = new Location(SellPos - new Vector3(0, 0, 5), new Vector3());

            Harvest_Range = 20f;

            BlipColor = Entities.Blips.BlipColor.Grey; 

            ItemIDBrute = ItemID.PetrolBrute;
            ItemIDProcess = ItemID.Petrol;
            ItemPrice = 21;
            Harvest_Time = 2600; // Prod : 2600
            Process_Time = 2400; // prod 2400
            Selling_Time = 600; // prod /  600
            Enabled = true;
        }

        public override void Init()
        {
            base.Init();
            RaffinerieColshape = ColshapeManager.CreateCylinderColshape(RaffineriePos, 10, 4);
            RaffinerieColshape.OnPlayerEnterColshape += OnPlayerEnterColshape;

            SellingColshape = ColshapeManager.CreateCylinderColshape(SellPos, 10, 4);
            SellingColshape.OnPlayerEnterColshape += OnPlayerEnterColshape;

            Marker.CreateMarker(MarkerType.VerticalCylinder, RaffineriePos - new Vector3(0,0,1), new Vector3(10, 10, 2), Color.FromArgb(0,0,0));
            Marker.CreateMarker(MarkerType.VerticalCylinder, Selling_PosRot.Pos - new Vector3(0,0,0), new Vector3(10, 10, 2), Color.FromArgb(0,0,0));

            GameMode.Instance.Streamer.AddEntityTextLabel("~o~Appuyez sur E en dehors du véhicule\n pour lancer le remplissage", Harvest_Position[0]);
        }

        public override void OnPlayerEnterColshape(IColshape colshape, IPlayer player)
        {
            if (RaffinerieColshape.IsEntityIn(player))
            {
                IVehicle vehicle = null;
                List<IVehicle> vehs = Process_PosRot.Pos.GetVehiclesInRange(20);

                foreach (IVehicle veh in vehs)
                {
                    if (veh.GetVehicleHandler().hasTrailer)
                        vehicle = (IVehicle)(veh.GetVehicleHandler().Trailer);
                }

                if (vehicle != null && Array.IndexOf(allowedTrailers, (VehicleModel)vehicle.Model) != -1)
                {
                    TankFilling(player, vehicle);
                }
                else
                    StartProcessing(player);

            }
            else if (SellingColshape.IsEntityIn(player))
                StartSelling(player);


            base.OnPlayerEnterColshape(colshape, player);
        }

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

            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDProcess);
            player.IsOnProgress = true;
            client.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);
            client.EmitLocked("LaunchProgressBar", FillingTime * (TrailerMaxContent / 10));
            Timers[client] = Utils.Utils.SetInterval(() =>
            {
                System.Timers.Timer ProcessTimer = Timers[client];
                if (!client.Exists || !trailer.Exists)
                    return;

                if (RaffineriePos.DistanceTo(trailer.Position) > 30f)
                {
                    client.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous deviez rester dans la zone.", 5000);
                    ProcessTimer.Stop();
                    ProcessTimer = null;
                }

                if (vehHandler.OilTank.Traite >= TrailerMaxContent)
                {
                    client.EmitLocked("StopProgressBar");
                    client.DisplaySubtitle($"Remplicage de la citerne terminé: Vous avez rempli ~r~ {vehHandler.OilTank.Traite}L {_itemTraite.name}(s)", 15000);
                    player.UpdateFull();
                    vehHandler.UpdateInBackground();
                    player.IsOnProgress = false;
                    ProcessTimer.Stop();
                    ProcessTimer = null;
                    return;
                }

                if (ProcessTimer == null)
                {
                    client.EmitLocked("StopProgressBar");
                    player.IsOnProgress = false;
                    return;
                }

                vehHandler.OilTank.Traite += 5;
                client.DisplaySubtitle("Volume essence raffiné : " + vehHandler.OilTank.Traite + "L", 3000);
            }, (Process_Time * 1000) / (FillingTime));
        }

        #region Start Farming

        public override void StartFarming(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            IVehicle fuelVeh = VehiclesManager.GetNearestVehicle(client.Position, 5, GameMode.GlobalDimension);

            if (fuelVeh == null || !fuelVeh.Exists)
            {
                client.DisplaySubtitle("~r~Aucun camion citerne à proximité", 5000);
                return;
            }
            else if (fuelVeh.Model != 4097861161)
            {
                client.SendNotificationError("Euh c'est pas un camion citerne.");
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
            else if (vehHandler.OilTank.Traite > 0)
            {
                client.DisplaySubtitle($"~r~Votre citerne contient déjà du pétrole raffiné.", 5000);
                return;
            }
            else if (vehHandler.OilTank.Brute >= 500)
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
            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            client.DisplaySubtitle($"Remplissage en cours de la citerne de {item.name}", 5000);

            bool exit = false;
            int needed = 500 - vehHandler.OilTank.Brute;
            client.EmitLocked("LaunchProgressBar", Harvest_Time * (needed / 10));

            Task.Run(async () =>
            {
                for (int i = 0; !exit; i++)
                {
                    if (vehHandler.OilTank.Brute < 500)
                        await Task.Delay(Harvest_Time);

                    if (!await client.ExistsAsync() || !await fuelVeh.ExistsAsync())
                        return;

                    if (await client.IsInVehicleAsync())
                    {
                        client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                        exit = true;
                    }

                    if ((await fuelVeh.GetPositionAsync()).Distance(lastPos) > Harvest_Range)
                    {
                        client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous devez rester dans la zone.", 5000);
                        exit = true;
                    }

                    if (exit)
                    {
                        player.IsOnProgress = false;
                        fuelVeh.ResetData("Refuelling");
                        client.EmitLocked("StopProgressBar");
                        return;
                    }

                    vehHandler.OilTank.Brute += 10;

                    if (vehHandler.OilTank.Brute >= 500)
                    {
                        fuelVeh.ResetData("Refuelling");
                        client.EmitLocked("StopProgressBar");
                        client.DisplaySubtitle($"Récolte terminée: la citerne est pleine ~r~ ({vehHandler.OilTank.Brute}L {item.name})", 10000);
                        player.IsOnProgress = false;
                        return;
                    }
                }
            });

            player.UpdateFull();
            vehHandler.UpdateInBackground();
        }

        /*
        public override void StartFarming(IPlayer client)
        {
            if (client == null || ! client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            if(client.Vehicle != null)
            {
                client.DisplayHelp("Vous ne pouvez pas faire cette action dans un véhicule.", 10000);
                return;
            }

            IVehicle fuelVeh = null;
            List<IVehicle> vehs = null;

            try
            {
                vehs = client.GetVehiclesInRange(20);
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }

            if (vehs == null)
                return;

            foreach(IVehicle veh in vehs)
            {
                if (!veh.Exists)
                    continue;

                if (veh.Model == 4097861161)
                {
                    client.DisplayHelp("Ce véhicule n'est plus homologué pour l'essence!", 10000);
                }

                if (veh.GetVehicleHandler().hasTrailer)
                    fuelVeh = (IVehicle)(veh.GetVehicleHandler().Trailer);
            }

            if (fuelVeh == null || !fuelVeh.Exists)
            {
                client.DisplayHelp("Aucune remorque à proximité", 5000);
                return;
            }
            else if (fuelVeh.HasData("Refueling"))
            {
                client.DisplayHelp("La remorque est déjà en remplissage.", 5000);
                return;
            }

            if (Array.IndexOf(allowedTrailers, fuelVeh.Model) == -1)
            {
                client.DisplayHelp("La remorque n'est pas homologuée pour cette action!", 10000);
                return;
            }

            VehicleHandler vehHandler = fuelVeh.GetVehicleHandler();

            if (vehHandler == null)
                return;

            else if (vehHandler.OilTank.Traite > 0)
            {
                client.DisplaySubtitle($"~r~Votre citerne contient déjà du pétrole raffiné, pas de mélange possible!", 5000);
                return;
            }
            else if (vehHandler.OilTank.Brute >= TrailerMaxContent)
            {
                client.DisplaySubtitle("~r~La citerne est déjà pleine.", 5000);
                return;
            }
            else if (fuelVeh.TryGetData("Refuelling", out bool useless))
            {
                client.DisplaySubtitle("~r~La citerne est déjà en remplissage.", 5000);
                return;
            }

            player.IsOnProgress = true;
            Vector3 lastPos = fuelVeh.Position;
            fuelVeh.SetData("Refuelling", true);
            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            client.DisplaySubtitle($"Remplissage en cours de la citerne de {item.name}", 5000);

            int needed = TrailerMaxContent - vehHandler.OilTank.Brute;
            client.EmitLocked("LaunchProgressBar", Harvest_Time * (needed / 10));

            Timers[client] = Utils.Utils.Delay((Harvest_Time * 1000) / TrailerMaxContent  ,false, async () =>
            {
                System.Timers.Timer HarvestTimer = Timers[client];
                if (!await client.ExistsAsync() || !await fuelVeh.ExistsAsync())
                {
                    HarvestTimer.Stop();
                    HarvestTimer = null;
                }

                if (await client.IsInVehicleAsync())
                {
                    client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                    HarvestTimer.Stop();
                    HarvestTimer = null;
                }

                if ((await fuelVeh.GetPositionAsync()).Distance(lastPos) > Harvest_Range)
                {
                    client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous deviez rester dans la zone.", 5000);
                    HarvestTimer.Stop();
                    HarvestTimer = null;
                }

                if (vehHandler.OilTank.Brute >= TrailerMaxContent)
                {
                    fuelVeh.ResetData("Refuelling");
                    client.EmitLocked("StopProgressBar");
                    client.DisplaySubtitle($"Récolte terminée: la citerne est pleine ~r~ ({vehHandler.OilTank.Brute}L {item.name})", 10000);
                    HarvestTimer.Stop();
                    HarvestTimer = null;
                    return;
                }

                if (HarvestTimer == null)
                {
                    player.IsOnProgress = false;
                    fuelVeh.ResetData("Refuelling");
                    client.EmitLocked("StopProgressBar");
                    return;
                }

                vehHandler.OilTank.Brute += 10;
                client.DisplayHelp("Volume dans la remorque: " + vehHandler.OilTank.Brute +"L",2000);

            });

            player.UpdateFull();
            vehHandler.UpdateInBackground();
        }*/



        #endregion

        #region Start Traitement
        public override void StartProcessing(IPlayer sender)
        {
            if (sender == null || !sender.Exists)
                return;

            PlayerHandler player = sender.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            IVehicle vehicle = sender.Vehicle;

            if (vehicle == null || !vehicle.Exists || vehicle.Driver != sender)
                return;

            VehicleHandler vehHandler = vehicle.GetVehicleHandler();

            if (vehHandler == null)
                return;
            else if (vehicle.Model != 4097861161)
            {
                sender.DisplaySubtitle($"Vous devez être dans un camion citerne.", 5000);
                return;
            }
            else if (vehHandler.OilTank.Brute == 0)
            {
                sender.DisplaySubtitle($"Votre camion citerne est vide.", 5000);
                return;
            }

            MenuManager.CloseMenu(sender);

            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);
            sender.EmitLocked("LaunchProgressBar", Process_Time * (vehHandler.OilTank.Brute / 10));
            bool exit = false;

            Task.Run(async () =>
            {
                while (!exit)
                {
                    if (!await sender.ExistsAsync())
                        return;

                    if (vehHandler.OilTank.Brute >= 1)
                        await Task.Delay(Process_Time);

                    if (RaffineriePos.DistanceTo(vehicle.Position) > 30f)
                    {
                        sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                        exit = true;
                    }

                    if (exit)
                    {
                        sender.EmitLocked("StopProgressBar");
                        player.IsOnProgress = false;
                        return;
                    }

                    vehHandler.OilTank.Brute -= 10;
                    vehHandler.OilTank.Traite += 5;

                    if (vehHandler.OilTank.Brute == 0)
                    {
                        sender.EmitLocked("StopProgressBar");
                        sender.DisplaySubtitle($"Traitement de la citerne terminé: Vous avez traité ~r~ {vehHandler.OilTank.Traite}L {_itemTraite.name}(s)", 15000);
                        player.UpdateFull();
                        vehHandler.UpdateInBackground();
                        player.IsOnProgress = false;
                        return;
                    }
                }
            });
        }

        /*
    public override void StartProcessing(IPlayer client)
    {
        if (client == null || !client.Exists)
            return;

        PlayerHandler player = client.GetPlayerHandler();

        if (player == null || player.IsOnProgress)
            return;

        if (client.Vehicle != null)
            client.DisplayHelp("Vous êtes libre de sortir du véhicule le temps du traitement!", 15000);

        IVehicle vehicle = null;
        List<IVehicle> vehs = Process_PosRot.Pos.GetVehiclesInRange(20);

        foreach (IVehicle veh in vehs)
        {

            if (veh.Model == 4097861161)
            {
                client.DisplayHelp("Ce véhicule n'est plus homologué pour l'essence!", 10000);
            }

            if (veh.GetVehicleHandler().hasTrailer)
                vehicle = (IVehicle)(veh.GetVehicleHandler().Trailer);

        }


        if (vehicle == null || !vehicle.Exists)
        {
            client.DisplayHelp("Aucune remorque à proximité", 5000);
            return;
        }
        else if (vehicle.HasData("Refueling"))
        {
            client.DisplayHelp("La remorque est déjà en traitement.", 5000);
            return;
        }

        if (Array.IndexOf(allowedTrailers, vehicle.Model) == -1)
        {
            client.DisplayHelp("La remorque n'est pas homologuée pour cette action!", 10000);
            return;
        }

        VehicleHandler vehHandler = vehicle.GetVehicleHandler();

        if (vehHandler == null)
            return;

        if (vehHandler.OilTank.Brute == 0)
        {
            client.DisplayHelp($"Vous n'avez rien à traiter", 5000);
            return;
        }

        MenuManager.CloseMenu(client);

        Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
        Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
        player.IsOnProgress = true;
        client.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);
        client.EmitLocked("LaunchProgressBar", Process_Time * (vehHandler.OilTank.Brute / 10));
        Timers[client] = Utils.Utils.Delay((Process_Time * 1000) / (Process_Time), false, () =>
        {
            System.Timers.Timer ProcessTimer = Timers[client];
            if (!client.Exists || !vehicle.Exists)
                return;

            if (RaffineriePos.DistanceTo(vehicle.Position) > 30f)
            {
                client.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous deviez rester dans la zone.", 5000);
                ProcessTimer.Stop();
                ProcessTimer = null;
            }


            if (vehHandler.OilTank.Brute == 0)
            {
                client.EmitLocked("StopProgressBar");
                client.DisplaySubtitle($"Traitement de la citerne terminé: Vous avez traité ~r~ {vehHandler.OilTank.Traite}L {_itemTraite.name}(s)", 15000);
                player.UpdateFull();
                vehHandler.UpdateInBackground();
                player.IsOnProgress = false;
                ProcessTimer.Stop();
                ProcessTimer = null;
                return;
            }

            if (ProcessTimer == null)
            {
                client.EmitLocked("StopProgressBar");
                player.IsOnProgress = false;
                return;
            }

            vehHandler.OilTank.Brute -= 10;
            vehHandler.OilTank.Traite += 5;
            client.DisplayHelp("Volume essence brute : " + vehHandler.OilTank.Brute + "L\n" + "Volume essence traité : " + vehHandler.OilTank.Traite + "L", 3000);
        });
    }*/
        #endregion


        public override void StartSelling(IPlayer sender)
        {
            if (sender == null || !sender.Exists)
                return;

            PlayerHandler player = sender.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;
            else if (!sender.IsInVehicle)
            {
                sender.DisplaySubtitle($"Vous devez être dans votre camion citerne.", 5000);
                return;
            }

            IVehicle vehicle = sender.Vehicle;

            if (vehicle == null || !vehicle.Exists || vehicle.Driver != sender)
                return;

            VehicleHandler vehHandler = vehicle.GetVehicleHandler();

            if (vehHandler == null)
                return;
            else if (vehicle.Model != 4097861161)
            {
                sender.DisplaySubtitle($"Vous devez être dans un camion citerne.", 5000);
                return;
            }
            else if (vehHandler.OilTank.Traite == 0)
            {
                sender.DisplaySubtitle($"Vous camion citerne est vide.", 5000);
                return;
            }
            else if (vehHandler.OilTank.Brute > 0)
            {
                sender.DisplaySubtitle($"Ce n'est pas du pétrole raffiné!", 5000);
                return;
            }

            MenuManager.CloseMenu(sender);

            Item _itemBuy = Inventory.Inventory.ItemByID(ItemIDProcess);
            player.IsOnProgress = true;

            sender.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

            int itemcount = vehHandler.OilTank.Traite;
            sender.EmitLocked("LaunchProgressBar", Selling_Time * itemcount);

            var count = 0;
            bool exit = false;

            Task.Run(async () =>
            {
                while (!exit)
                {
                    if (!sender.Exists)
                        return;

                    if (vehHandler.OilTank.Traite >= 1)
                        await Task.Delay(Selling_Time);

                    if (SellPos.DistanceTo(vehicle.Position) > 30f)
                    {
                        sender.DisplaySubtitle($"~r~Vente interrompue: ~s~Vous devez rester dans la zone.", 5000);
                        exit = true;
                    }

                    if (exit)
                    {
                        sender.EmitLocked("StopProgressBar");
                        player.IsOnProgress = false;
                        return;
                    }

                    count++;
                    vehHandler.OilTank.Traite--;

                    if (vehHandler.OilTank.Traite == 0)
                    {
                        sender.EmitLocked("StopProgressBar");
                        double gettaxe = Economy.Economy.CalculPriceTaxe(ItemPrice * itemcount, GameMode.Instance.Economy.Taxe_Exportation);
                        player.AddMoney((ItemPrice * itemcount) - gettaxe);
                        GameMode.Instance.Economy.CaissePublique += gettaxe;
                        sender.DisplaySubtitle($"~r~{itemcount} ~w~{_itemBuy.name}(s) $~r~{(ItemPrice * itemcount) - gettaxe} ~w~taxe:$~r~{gettaxe}.", 15000);
                        player.IsOnProgress = false;
                        return;
                    }
                }

                player.IsOnProgress = false;
                player.UpdateFull();
            });
        }

        /*
    public override void StartSelling(IPlayer client)
    {
        if (client == null || !client.Exists)
            return;

        PlayerHandler player = client.GetPlayerHandler();

        if (player == null || player.IsOnProgress)
            return;

        if (client.Vehicle != null)
            client.DisplayHelp("Vous êtes libre de sortir du véhicule le temps de la vente!", 15000);

        IVehicle vehicle = null;
        List<IVehicle> vehs = Selling_PosRot.Pos.GetVehiclesInRange(20);

        foreach (IVehicle veh in vehs)
        {

            if (veh.Model == 4097861161)
            {
                client.DisplayHelp("Ce véhicule n'est plus homologué pour l'essence!", 10000);
            }

            if (veh.GetVehicleHandler().hasTrailer)
                vehicle = (IVehicle)(veh.GetVehicleHandler().Trailer);

        }

        if (vehicle == null || !vehicle.Exists)
        {
            client.DisplayHelp("Aucune remorque à proximité", 5000);
            return;
        }
        else if (vehicle.HasData("Refueling"))
        {
            client.DisplayHelp("La remorque est déjà en traitement.", 5000);
            return;
        }

        if (Array.IndexOf(allowedTrailers, vehicle.Model) == -1)
        {
            client.DisplayHelp("La remorque n'est pas homologuée pour cette action!", 10000);
            return;
        }

        VehicleHandler vehHandler = vehicle.GetVehicleHandler();

        if (vehHandler == null)
            return;
        else if (vehHandler.OilTank.Traite == 0)
        {
            client.DisplaySubtitle($"Vous camion citerne est vide.", 5000);
            return;
        }
        else if (vehHandler.OilTank.Brute > 0)
        {
            client.DisplaySubtitle($"Ce n'est pas du pétrole raffiné!", 5000);
            return;
        }

        MenuManager.CloseMenu(client);

        Item _itemBuy = Inventory.Inventory.ItemByID(ItemIDProcess);
        player.IsOnProgress = true;

        client.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

        int itemcount = vehHandler.OilTank.Traite;
        client.EmitLocked("LaunchProgressBar", Selling_Time * itemcount);

        int count = 0;

        Timers[client] = Utils.Utils.Delay(Selling_Time * 1000 / TrailerMaxContent, false, () =>
        {
            if (!client.Exists)
                return;

            System.Timers.Timer SellingTimer = Timers[client];

            if (SellPos.DistanceTo(vehicle.Position) > 30f)
            {
                client.DisplaySubtitle($"~r~Vente interrompue: ~s~Vous devez rester dans la zone.", 5000);
                SellingTimer.Stop();
                SellingTimer = null;
            }

            if (vehHandler.OilTank.Traite == 0)
            {
                client.EmitLocked("StopProgressBar");
                player.IsOnProgress = false;
                SellingTimer.Stop();
                SellingTimer = null;
                return;
            }

            if (SellingTimer == null)
            {
                client.EmitLocked("StopProgressBar");
                double gettaxe = Economy.Economy.CalculPriceTaxe(ItemPrice * count, GameMode.Instance.Economy.Taxe_Exportation);
                player.AddMoney((ItemPrice * count) - gettaxe);
                GameMode.Instance.Economy.CaissePublique += gettaxe;
                client.DisplaySubtitle($"~r~{count} ~w~{_itemBuy.name}(s) $~r~{(ItemPrice * count) - gettaxe} ~w~taxe:$~r~{gettaxe}.", 15000);
                player.IsOnProgress = false;
                player.UpdateFull();
                return;
            }

            count++;
            vehHandler.OilTank.Traite--;
            client.DisplayHelp("Volume vendu: " + count + "L\nArgent gagné : " + ItemPrice * count + "$", 3000);
        });
    }*/



    }
}
