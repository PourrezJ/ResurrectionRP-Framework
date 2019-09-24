using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Extensions;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Farms
{
    public class Petrol : Farm
    {
        private static Vector3 RaffineriePos = new Vector3(2782.841f, 1704.716f, 23.83286f);
        private static Vector3 SellPos = new Vector3(302.9887f, -2756.656f, 5.424667f);

        public IColShape RaffinerieColshape = null;
        public IColShape SellingColshape = null;

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

        public override async Task Init()
        {
            await base.Init();
            RaffinerieColshape = AltV.Net.Alt.CreateColShapeCylinder(RaffineriePos, 20, 4);
            await RaffinerieColshape.SetDimensionAsync(GameMode.GlobalDimension);
            RaffinerieColshape.SetOnPlayerEnterColShape(OnPlayerColshape);


            SellingColshape = AltV.Net.Alt.CreateColShapeCylinder(SellPos, 20, 4);
            await SellingColshape.SetDimensionAsync(GameMode.GlobalDimension);
            SellingColshape.SetOnPlayerEnterColShape(OnPlayerColshape);

            Marker.CreateMarker(MarkerType.VerticalCylinder, RaffineriePos - new Vector3(0,0,1), new Vector3(10, 10, 2), Color.FromArgb(0,0,0));
        }

        public override async Task OnPlayerColshape(IColShape colShape, IPlayer player)
        {
            if (RaffinerieColshape.IsEntityInColShape(player))
            {
                await StartProcessing(player);
            }
            else if (SellingColshape.IsEntityInColShape(player))
            {
                await StartSelling(player);
            }


            await base.OnPlayerColshape(colShape, player);
        }

        #region Start Farming
        public override async Task StartFarming(IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player.IsOnProgress)
                return;

            IVehicle fuelveh = await VehiclesManager.GetNearestVehicle(client, 5, GameMode.GlobalDimension);

            if (fuelveh == null)
            {
                client.DisplaySubtitle("~r~Aucun camion citerne à proximité", 5000);
                return;
            }

            if (await fuelveh.GetModelAsync() != 4097861161)
            {
                client.SendNotificationError("Euh c'est pas un camion citerne.");
                return;
            }

            if (fuelveh.TryGetData("Refueling", out object data))
            {
                client.DisplaySubtitle("~r~Le camion est déjà en remplissage.", 5000);
                return;
            }

            if (await client.IsInVehicleAsync())
            {
                client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                return;
            }

            if (!fuelveh.Exists)
                return;

            var vehHandler = fuelveh.GetVehicleHandler();
            if (vehHandler == null)
                return;

            if (vehHandler.OilTank.Traite > 0)
            {
                client.DisplaySubtitle($"~r~Votre citerne contient déjà du pétrole raffiné.", 5000);
                return;
            }

            if (vehHandler.OilTank.Brute >= 500)
            {
                client.DisplaySubtitle("~r~Le camion est déjà rempli.", 5000);
                return;
            }

            if (fuelveh.TryGetData("Refuelling", out bool useless))
            {
                client.DisplaySubtitle("~r~Le camion est déjà en remplissage.", 5000);
                return;
            }


            client.GetPlayerHandler().IsOnProgress = true;
            VehicleHandler vh = fuelveh.GetVehicleHandler();
            Vector3 lastPos = await fuelveh.GetPositionAsync();
            fuelveh.SetData("Refuelling", true);
            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            client.DisplaySubtitle($"Remplissage en cours de la citerne de {item.name}", 5000);

            bool _exit = false;
            var needed = 500 - vehHandler.OilTank.Brute;

            client.EmitLocked("LaunchProgressBar", Harvest_Time * (needed / 10));

            for (int i = 0; !_exit; i++)
            {
                if (vehHandler.OilTank.Brute < 500)
                    await Task.Delay(Harvest_Time);

                if (!client.Exists)
                    return;

                if (!fuelveh.Exists)
                    return;

                if (await client.IsInVehicleAsync())
                {
                    client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                    _exit = true;
                }

                if ((await fuelveh.GetPositionAsync()).Distance(lastPos) > Harvest_Range)
                {
                    client.DisplaySubtitle($"~r~Récolte interrompue: ~s~Vous devez rester dans la zone.", 5000);
                    _exit = true;
                }

                if (_exit)
                {
                    player.IsOnProgress = false;
                    fuelveh.ResetData("Refuelling");
                    client.EmitLocked("StopProgressBar");
                    return;
                }

                vehHandler.OilTank.Brute += 10;

                if (vehHandler.OilTank.Brute >= 500)
                {
                    _exit = true;
                    fuelveh.ResetData("Refuelling");
                    client.EmitLocked("StopProgressBar");
                    client.DisplaySubtitle($"Récolte terminée: la citerne est pleine ~r~ ({vehHandler.OilTank.Brute}L {item.name})", 10000);
                    player.IsOnProgress = false;
                    return;
                }
            }

            player.UpdateFull();
            vehHandler.UpdateFull();
        }
        #endregion

        #region Start Traitement
        public override async Task StartProcessing(IPlayer sender)
        {
            PlayerHandler player = sender.GetPlayerHandler();
            if (player.IsOnProgress) return;

            if (!await sender.IsInVehicleAsync())
            {
                sender.DisplaySubtitle($"Vous devez être dans votre camion citerne.", 5000);
                return;
            }

            var vehicle = await sender.GetVehicleAsync();
            
            if (!vehicle.Exists)
                return;

            if (await vehicle.GetDriverAsync() != sender)
                return;
        
            var vehHandler = vehicle.GetVehicleHandler();

            if (vehHandler == null)
                return;

            if (await vehicle.GetModelAsync() != 4097861161)
            {
                sender.DisplaySubtitle($"Vous devez être dans un camion citerne.", 5000);
                return;
            }

            if (vehHandler.OilTank.Brute == 0)
            {
                sender.DisplaySubtitle($"Votre camion citerne est vide.", 5000);
                return;
            }

            await MenuManager.CloseMenu(sender);
            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);

            sender.EmitLocked("LaunchProgressBar", Process_Time * (vehHandler.OilTank.Brute / 10));

            bool _exit = false;
            while (!_exit)
            {
                if (!sender.Exists)
                    return;

                if (vehHandler.OilTank.Brute >= 1)
                    await Task.Delay(Process_Time);

                if (RaffineriePos.DistanceTo(await vehicle.GetPositionAsync()) > 30f)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    _exit = true;
                }

                if (_exit)
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
                    vehHandler.UpdateFull();
                    player.IsOnProgress = false;
                    return;
                }
            }
        }
        #endregion

        public override async Task StartSelling(IPlayer sender)
        {
            PlayerHandler player = sender.GetPlayerHandler();
            if (player.IsOnProgress)
                return;

            if (!await sender.IsInVehicleAsync())
            {
                sender.DisplaySubtitle($"Vous devez être dans votre camion citerne.", 5000);
                return;
            }

            var vehicle = await sender.GetVehicleAsync();

            if (!vehicle.Exists)
                return;

            if (await vehicle.GetDriverAsync() != sender)
                return;

            var vehHandler = vehicle.GetVehicleHandler();

            if (vehHandler == null)
                return;

            if (await vehicle.GetModelAsync() != 4097861161)
            {
                sender.DisplaySubtitle($"Vous devez être dans un camion citerne.", 5000);
                return;
            }

            if (vehHandler.OilTank.Traite == 0)
            {
                sender.DisplaySubtitle($"Vous camion citerne est vide.", 5000);
                return;
            }

            if (vehHandler.OilTank.Brute > 0)
            {
                sender.DisplaySubtitle($"Ce n'est pas du pétrole raffiné!", 5000);
                return;
            }

            await MenuManager.CloseMenu(sender);
            Item _itemBuy = Inventory.Inventory.ItemByID(ItemIDProcess);
            player.IsOnProgress = true;

            sender.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

            await MenuManager.CloseMenu(sender);
            int itemcount = vehHandler.OilTank.Traite;

            sender.EmitLocked("LaunchProgressBar", Selling_Time * itemcount);

            var count = 0;
            bool _exit = false;
            while (!_exit)
            {
                if (!sender.Exists)
                    return;

                if (vehHandler.OilTank.Traite >= 1)
                    await Task.Delay(Selling_Time);

                if (SellPos.DistanceTo(await vehicle.GetPositionAsync()) > 30f)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    _exit = true;
                }

                if (_exit)
                {
                    sender.EmitLocked("StopProgressBar");
                    player.IsOnProgress = false;
                    return;
                }

                count++;
                vehHandler.OilTank.Traite --;
                if (vehHandler.OilTank.Traite == 0)
                {
                    sender.EmitLocked("StopProgressBar");          
                    double gettaxe = Economy.Economy.CalculPriceTaxe((ItemPrice * itemcount), GameMode.Instance.Economy.Taxe_Exportation);
                    player.AddMoney((ItemPrice * itemcount) - gettaxe);
                    GameMode.Instance.Economy.CaissePublique += gettaxe;
                    sender.DisplaySubtitle($"~r~{itemcount} ~w~{_itemBuy.name}(s) $~r~{(ItemPrice * itemcount) - gettaxe} ~w~taxe:$~r~{gettaxe}.", 15000);
                    player.IsOnProgress = false;
                    return;
                }
            }

            player.IsOnProgress = false;
            player.UpdateFull();
        }
    }
}
