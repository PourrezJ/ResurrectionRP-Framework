using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
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

        private static int TrailerMaxContent = 500;

        public static uint[] allowedTrailers = new uint[3] { 0xB8081009, 0xD46F4737, 0x74998082 };

        public IColShape RaffinerieColshape = null;
        public IColShape SellingColshape = null;

        public ConcurrentDictionary<IPlayer, System.Timers.Timer> Timers = new ConcurrentDictionary<IPlayer, System.Timers.Timer>();

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
            RaffinerieColshape = AltV.Net.Alt.CreateColShapeCylinder(RaffineriePos, 10, 4);
            RaffinerieColshape.Dimension = GameMode.GlobalDimension;
            RaffinerieColshape.SetOnPlayerEnterColShape(OnPlayerEnterColshape);


            SellingColshape = AltV.Net.Alt.CreateColShapeCylinder(SellPos, 10, 4);
            SellingColshape.Dimension = GameMode.GlobalDimension;
            SellingColshape.SetOnPlayerEnterColShape(OnPlayerEnterColshape);

            Marker.CreateMarker(MarkerType.VerticalCylinder, RaffineriePos - new Vector3(0,0,1), new Vector3(10, 10, 2), Color.FromArgb(0,0,0));
            Marker.CreateMarker(MarkerType.VerticalCylinder, Selling_PosRot.Pos - new Vector3(0,0,0), new Vector3(10, 10, 2), Color.FromArgb(0,0,0));

            GameMode.Instance.Streamer.AddEntityTextLabel("~o~Appuyez sur E en dehors du véhicule\n pour lancer le remplissage", Harvest_Position[0]);
        }

        public override void OnPlayerEnterColshape(IColShape colShape, IPlayer player)
        {
            if (RaffinerieColshape.IsEntityInColShape(player))
                StartProcessing(player);
            else if (SellingColshape.IsEntityInColShape(player))
                StartSelling(player);


            base.OnPlayerEnterColshape(colShape, player);
        }

        #region Start Farming
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
            List<IVehicle> vehs = client.GetVehiclesInRange(20);

            foreach(IVehicle veh in vehs)
            {

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

            client.GetPlayerHandler().IsOnProgress = true;
            Vector3 lastPos = fuelVeh.Position;
            fuelVeh.SetData("Refuelling", true);
            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            client.DisplaySubtitle($"Remplissage en cours de la citerne de {item.name}", 5000);

            int needed = TrailerMaxContent - vehHandler.OilTank.Brute;
            client.EmitLocked("LaunchProgressBar", Harvest_Time * (needed / 10));

            Timers[client] = Utils.Utils.Delay((Harvest_Time * 1000) / TrailerMaxContent  ,false, async () =>
            {
                System.Timers.Timer HarvestTimer = Timers[client];
                if (!client.Exists || !fuelVeh.Exists)
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

                if ((fuelVeh.Position).Distance(lastPos) > Harvest_Range)
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
        }
        #endregion

        #region Start Traitement
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
        }
        #endregion

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
        }
    }
}
