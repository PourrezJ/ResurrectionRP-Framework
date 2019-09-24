using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.XMenuManager;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{

    public class LSCustom : Faction
    {
        #region Fields
        public static Vector3 DepotVehicle = new Vector3(-357.0305f, -134.4201f, 38.93036f);
        public static Location ReparZoneVL = new Location(new Vector3(-325.1813f, -134.0631f, 38.5204f), new Vector3(-0.992146f, -1.029223f, 306.1207f));

        public static Vector3 PeintureZone = new Vector3(-326.7145f, -144.8862f, 37.55893f);

        private static readonly int ReparEnginePrice = 900;
        private static readonly int ReparBody = 700;
        private static readonly int ReparFortune = 1000;
        private static readonly int PeinturePrice = 2000;
        private static readonly int ClearVehicle = 0;

        [BsonIgnore]
        private IVehicle VehicleInWorkbench;

        [BsonIgnore]
        private IVehicle VehicleInColorCabin;

        [BsonIgnore]
        public IColShape ReparationVLColshape { get; private set; }
        [BsonIgnore]
        public IColShape PeintureColshape { get; private set; }
        #endregion

        #region Constructor
        public LSCustom(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }
        #endregion

        #region Event listeners
        public override async Task<Faction> OnFactionInit()
        {
            Vector3 reparZone = new Vector3(-324.7894f, -134.2555f, 35.54341f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, reparZone, new Vector3(4, 4, 1), System.Drawing.Color.White, GameMode.GlobalDimension);
            ReparationVLColshape = Alt.CreateColShapeCylinder(reparZone, 4, 4);
            ReparationVLColshape.Dimension = GameMode.GlobalDimension;
            ReparationVLColshape.SetOnPlayerInteractInColShape(OnEnterReparZoneVL);
            ReparationVLColshape.SetOnPlayerEnterColShape(OnEnterColshapeInteract);
            ReparationVLColshape.SetOnVehicleEnterColShape(OnVehicleEnterColshape);
            ReparationVLColshape.SetOnVehicleLeaveColShape(OnVehicleQuitColshape);

            PeintureColshape = Alt.CreateColShapeCylinder(PeintureZone, 4, 4);
            PeintureColshape.Dimension = GameMode.GlobalDimension;
            Marker.CreateMarker(MarkerType.VerticalCylinder, PeintureZone, new Vector3(4, 4, 1), System.Drawing.Color.White, GameMode.GlobalDimension);
            PeintureColshape.SetOnPlayerInteractInColShape(OnEnterPaintZoneVL);
            PeintureColshape.SetOnPlayerEnterColShape(OnEnterColshapeInteract);
            PeintureColshape.SetOnVehicleEnterColShape(OnVehicleEnterColshape);
            PeintureColshape.SetOnVehicleLeaveColShape(OnVehicleQuitColshape);

            FactionRang = new FactionRang[] {
                new FactionRang(0,"Dépanneur", false, 2500, true),
                new FactionRang(1,"Bras Droit", false, 3000, true),
                new FactionRang(2,"Gérant", true, 3500, true, true)
            };

            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Flatbed, 230000));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.TowTruck, 150000));

            BlipPosition = new Vector3(-324.7894f, -134.2555f, 35.54341f);
            BlipColor = BlipColor.White;
            BlipSprite = 446;

            ParkingLocation = new Location(new Vector3(-368.9921f, -108.6537f, 38.68017f), new Vector3(0, 0, 70.15666f));
            ServiceLocation = new Vector3(-347.0162f, -133.5529f, 39.00962f);
            ShopLocation = new Vector3(-344.9704f, -123.2633f, 38.00966f);

            // Items SHOP
            ItemShop.Add(new FactionShop(new LockPick(ItemID.LockPick, "Kit De Crochetage", "", 0.2, true, false, true, true), 5000, 1));
            ItemShop.Add(new FactionShop(new CrateTools(ItemID.CrateTool, "Caisse a outil", "De marque Facom", 1, true, false, true, true), 15000, 1));

            return await base.OnFactionInit();
        }

        private async Task OnEnterPaintZoneVL(IColShape colShape, IPlayer client)
        {
            if (VehicleInColorCabin == null)
            {
                client.SendNotificationError("Le véhicule doit être dans la cabine de peinture pour être repeint.");
                return;
            }
            var employees = GetAllSocialClubName();
            var social = client.GetSocialClub();

            if (employees.Contains(social))
            {
                await OpenPeintureMenu(client);
            }    
        }

        private async Task OnVehicleQuitColshape(IColShape colShape, IVehicle vehicle)
        {
            if (colShape == ReparationVLColshape)
            {
                VehicleInWorkbench = null;
            }
            else if (colShape == PeintureColshape)
            {
                VehicleInColorCabin = null;
            }

            if (vehicle.Driver != null)
                await MenuManager.CloseMenu(vehicle.Driver);
        }

        private Task OnVehicleEnterColshape(IColShape colShape, IVehicle vehicle)
        {
            if (colShape == ReparationVLColshape)
            {
                VehicleInWorkbench = vehicle;
            }
            else if (colShape == PeintureColshape)
            {
                VehicleInColorCabin = vehicle;
            }

            return Task.CompletedTask;
        }

        private Task OnEnterColshapeInteract(IColShape colShape, IPlayer client)
        {
            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
            return Task.CompletedTask;
        }

        private async Task OnPeintureSelect(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleInColorCabin == null)
            {
                client.SendNotificationError("Le véhicule doit être dans la cabine de peinture pour être repeint.");
                return;
            }

            menu = new Menu(menuItem.Id, "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.CarMod);
            menu.ItemSelectCallback = PeintureSelectCallback;
            menu.IndexChangeCallback = PeinturePreview;

            foreach (VehicleColor color in Enum.GetValues(typeof(VehicleColor)))
            {
                MenuItem item = new MenuItem(color.ToString(), executeCallback: true, executeCallbackIndexChange: true);
                item.RightLabel = $"${PeinturePrice}";
                item.SetData("Color", Convert.ToInt32(color));
                item.OnMenuItemCallback += OnColorChoise;
                menu.Add(item);
            }

            await menu.OpenMenu(client);
            await PeinturePreview(client, menu, 0, menu.Items[0]);
        }

        private async Task PeintureSelectCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem != null && VehicleInColorCabin != null) 
            {
                VehicleHandler vh = VehicleInColorCabin.GetVehicleHandler();
                VehicleInColorCabin.PrimaryColor = vh.PrimaryColor;
                VehicleInColorCabin.SecondaryColor = vh.SecondaryColor;
                await OpenPeintureMenu(client);
            }
        }

        private async Task OnColorChoise(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            int color = menuItem.GetData("Color");
            if (VehicleInColorCabin == null)
                return;

            var ph = client.GetPlayerHandler();
            if (ph == null)
                return;

            var vh = VehicleInColorCabin.GetVehicleHandler();
            if (vh == null)
                return;

            if (await BankAccount.GetBankMoney(PeinturePrice, $"Peinture du véhicule {vh.Plate} par {ph.Identite.Name}"))
            {
                if (menu.Id == "ID_First")
                {
                    VehicleInColorCabin.PrimaryColor = ((byte)color);
                    vh.PrimaryColor = (byte)color;
                }
                else if (menu.Id == "ID_Second")
                {
                    VehicleInColorCabin.SecondaryColor = (byte)color;
                    vh.SecondaryColor = (byte)color;
                }
                else if (menu.Id == "ID_Pearl")
                {
                    VehicleInColorCabin.PearlColor = (byte)color;
                    vh.PearlColor = (byte)color;
                }

                vh.UpdateFull();
                client.SendNotificationSuccess("Peinture effectuée!");
            }
            else
            {
                client.SendNotificationError("Le fond de commerce est vide.");
            }
        }

        private async Task OnEnterReparZoneVL(IColShape colShape, IPlayer client)
        {
            if (VehicleInWorkbench != null)
                await OpenMenu(client, VehicleInWorkbench);
        }

        public override async Task OnPlayerServiceEnter(IPlayer client, int rang)
        {
            switch (client.GetPlayerHandler().Character.Gender)
            {
                case 0: // Homme
                    client.SetCloth(Models.ClothSlot.Undershirt, 8, 36, 0);
                    break;
                case 1: // Femme
                    client.SetCloth(Models.ClothSlot.Undershirt, 8, 36, 0);
                    break;
                default: // Ped?
                    client.SendNotificationError("Vous ne pouvez pas avoir de tenue avec ce personnage");
                    break;
            }
            await base.OnPlayerServiceEnter(client, rang);
        }

        public override async Task OnPlayerServiceQuit(IPlayer client, int rang)
        {
            client.GetPlayerHandler().Character.ApplyCharacter(client);
            // rendre la tenue
            await base.OnPlayerServiceQuit(client, rang);
        }
        #endregion

        #region Methods
        public async Task OpenPeintureMenu(IPlayer client)
        {
            if (VehicleInColorCabin == null)
            {
                client.SendNotificationError("Le véhicule doit être dans la cabine de peinture pour être repeint.");
                return;
            }
            Menu menu = new Menu("ID_MainReparMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.CarMod);

            MenuItem primary = new MenuItem("Peinture Principal", "", "ID_First", executeCallback: true, executeCallbackIndexChange: true);
            primary.OnMenuItemCallback = OnPeintureSelect;
            menu.Add(primary);

            MenuItem secondary = new MenuItem("Peinture Secondaire", "", "ID_Second", executeCallback: true, executeCallbackIndexChange: true);
            secondary.OnMenuItemCallback = OnPeintureSelect;
            menu.Add(secondary);

            MenuItem pearl = new MenuItem("Peinture Pearler", "", "ID_Pearl", executeCallback: true, executeCallbackIndexChange: true);
            pearl.OnMenuItemCallback = OnPeintureSelect;
            menu.Add(pearl);

            await menu.OpenMenu(client);
        }

        private Task PeinturePreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            int color = menuItem.GetData("Color");

            if (VehicleInColorCabin == null)
                return Task.CompletedTask;

            if (menu.Id == "ID_First")
                VehicleInColorCabin.PrimaryColor = (byte)color;
            else if (menu.Id == "ID_Second")
                VehicleInColorCabin.SecondaryColor = (byte)color;
            else if (menu.Id == "ID_Pearl")
                VehicleInColorCabin.PearlColor = (byte)color;
            return Task.CompletedTask;
        }
        #endregion

        #region Menus
        public override XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            var playerHandler = client.GetPlayerHandler();
            xmenu.SetData("Player", target);

            return base.InteractPlayerMenu(client, target, xmenu);
        }

        private async Task UseCrateTool(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            var playerHandler = client.GetPlayerHandler();

            if (playerHandler == null)
                return;

            if (playerHandler.GetStacksItems(ItemID.CrateTool).Count > 0)
            {
                var inventory = playerHandler.GetStacksItems(ItemID.CrateTool);

                if (inventory == null)
                    return;

                if (inventory.ContainsKey(InventoryTypes.Pocket) || inventory.ContainsKey(InventoryTypes.Bag))
                {
                    var vehicle = menu.GetData("Vehicle");
                    await CrateTools.RepairVehicle(client, vehicle);
                }
            }
        }

        public async Task OpenMenu(IPlayer client, IVehicle vehicle)
        {
            Menu menu = new Menu("ID_MainReparMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.CarMod);
            menu.SetData("Vehicle", vehicle);
            menu.ItemSelectCallback = MenuCallBack;

            if (HasPlayerIntoFaction(client))
            {
                menu.Add(new MenuItem("Diagnostique véhicule", "", "ID_Diag", true));
                menu.Add(new MenuItem("Réparer la carrosserie", "", "ID_Body", true));
                menu.Add(new MenuItem("Réparer le moteur", "", "ID_Engine", true));
                menu.Add(new MenuItem("Nettoyer le véhicule", "", "ID_Clean", true));
            }
            else if (GetEmployeeOnline().Count > 0)
                client.SendNotificationError("Vous devez passer par un mécanicien.");
            else
                menu.Add(new MenuItem("Bricoler le moteur", "Faites une réparation à 50% des dégâts moteur.", "ID_BricoEngine", true, rightLabel: $"${ReparFortune}"));

            await menu.OpenMenu(client);
        }


        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            IVehicle veh = menu.GetData("Vehicle");
            if (veh == null)
                return;

            if (!veh.Exists)
                return;

            PlayerHandler ph = client.GetPlayerHandler();
            VehicleHandler _vh = veh.GetVehicleHandler();

            switch (menuItem.Id)
            {
                case "ID_Diag":
                    client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Diagnostique: ~r~Démarrage~w~.", "En cours ...");

                    Utils.Utils.Delay(20000, true, async () =>
                    {
                        if (!veh.Exists || !client.Exists)
                            return;

                        string str =
                        "~r~Résultat:~w~\n" +
                        $"Chassis:   {Math.Floor(await veh.GetBodyHealthAsync() * 0.1)}% \n" +
                        $"Moteur:    {Math.Floor(await veh.GetEngineHealthAsync() * 0.1)}%\n" +
                        $"Réservoir: {Math.Floor(await veh.GetPetrolTankHealthAsync() * 0.1)}%\n";

                        client.DisplaySubtitle(str, 5000);
                    });

                    break;

                case "ID_Body":
                    if (await BankAccount.GetBankMoney(ReparBody, $"Réparation carrosserie {_vh.Plate} par {ph.Identite.Name}"))
                    {
                        _vh.UpdateProperties();

                        Utils.Utils.Delay(20000, true, () =>
                        {
                            if (client == null || !client.Exists)
                                return;

                            int engineHealth = _vh.EngineHealth;
                            int petrolTankHealth = _vh.PetrolTankHealth;
                            _vh.Repair(client);
                            _vh.EngineHealth = engineHealth;
                            _vh.PetrolTankHealth = petrolTankHealth;
                            _vh.UpdateAsync();
                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Carrosserie: ~g~Terminé~w~.", "Elle est niquel!");
                        });

                        await UpdateDatabase();
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");

                    break;

                case "ID_Engine":
                    if (await BankAccount.GetBankMoney(ReparEnginePrice, $"Réparation moteur {_vh.Plate} par {ph.Identite.Name}"))
                    {
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~r~Démarrage~w~.","C'est parti!");

                        Utils.Utils.Delay(20000, true, async () =>
                        {
                            if (!veh.Exists || !client.Exists)
                                return;

                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~g~Terminé~w~.","Il est niquel!");

                            await AltAsync.Do(() =>
                            {
                                _vh.EngineHealth = 1000;

                                for (byte i = 0; i < veh.WheelsCount; i++)
                                {
                                    veh.SetWheelBurst(i, _vh.Wheels[i].Burst);
                                    veh.SetWheelHealth(i, _vh.Wheels[i].Health);
                                    veh.SetWheelHasTire(i, _vh.Wheels[i].HasTire);
                                }

                                for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                                    veh.SetDoorState(i, (byte)_vh.Doors[i]);

                                for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                                {
                                    if (_vh.Windows[i] == WindowState.WindowBroken)
                                        veh.SetWindowDamaged(i, true);
                                    else if (_vh.Windows[i] == WindowState.WindowDown)
                                        veh.SetWindowOpened(i, true);
                                }
                            });
                                       
                            _vh.UpdateFull();
                            _vh.UpdateProperties();
                            await _vh.ApplyDamageAsync();
                        });

                        await UpdateDatabase();
                    }
                    else
                       client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");

                    break;

                case "ID_Clean":
                    if (await BankAccount.GetBankMoney(ClearVehicle, $"Néttoyage vehicule {_vh.Plate} par {ph.Identite.Name}"))
                    {
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Nettoyage: ~r~Démarrage~w~.","C'est parti!" );

                        Utils.Utils.Delay(20000, true, async () =>
                        {
                            if (!veh.Exists || !client.Exists)
                                return;

                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Nettoyage: ~g~Terminé~w~.","Elle est niquel!");
                            _vh.DirtLevel = 0;
                            _vh.UpdateFull();
                            await veh.SetDirtLevelAsync(0);
                            await _vh.ApplyDamageAsync();
                        });

                        await UpdateDatabase();
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");

                    break;

                case "ID_BricoEngine":
                    if (client.GetPlayerHandler().HasMoney(ReparFortune))
                    {
                        client.SendNotificationPicture( CharPicture.CHAR_LS_CUSTOMS ,"Los Santos Custom", "Réparation Moteur: ~r~Démarrage~w~.", "Alors ce tuyau va où déjà?");

                        Utils.Utils.Delay(20000, true, async () =>
                        {
                            if (!veh.Exists || !client.Exists)
                                return;

                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~g~Terminé~w~.","Le moteur démarre, c'est déjà ça!");
                            _vh.EngineHealth = 400;
                            _vh.UpdateFull();
                            await _vh.ApplyDamageAsync();
                        });

                        await UpdateDatabase();
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent sur vous!");

                    break;
            }
        }
        #endregion

        #region Static
        public static void TowVehicle(IVehicle towtruck, IVehicle vehicle)
        {
            VehicleHandler towhandler = towtruck.GetVehicleHandler();
            //towhandler.TowVehicle(towtruck, vehicle, new Vector3(0, -2, 1));
        }

        public static async Task<bool> IsWhitelistClassTow(IVehicle vehicle)
        {
            int[] whitelistClass = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 18 };

            var data = VehicleInfoLoader.VehicleInfoLoader.Get(await vehicle.GetModelAsync());
            if (data != null)
            {
                if ( (data.VehicleClass == 18 || data.VehicleClass <= 12 && data.VehicleClass >= 0) && data.VehicleClass <= 18)
                    return true;
            }
            return false;
        }

        public override async Task<XMenu> InteractVehicleMenu(IPlayer client, IVehicle target, XMenu xmenu)
        {
            xmenu.SetData("Vehicle", target);

            xmenu.Callback += MenuCallback;
            var nearest = (await client.GetPositionAsync()).GetTowTruckInZone(10f)?.GetVehicleHandler();

            if (await target.GetModelAsync() != (int)VehicleModel.Flatbed && await LSCustom.IsWhitelistClassTow(target) == true)
            {
                xmenu.Add(new XMenuItem("Remorquer", "", "ID_attach", XMenuItemIcons.TRUCK_LOADING_SOLID, true));
            }

            if (await target.GetModelAsync() == (int)VehicleModel.Flatbed && target.GetVehicleHandler().TowTruck != null)
            {
                xmenu.Add(new XMenuItem("Détacher", "", "ID_detach", XMenuItemIcons.TRUCK_LOADING_SOLID, true));
            }

            if (FactionManager.IsLSCustom(client) && await target.GetModelAsync() == (uint)VehicleModel.Flatbed && target.GetVehicleHandler().TowTruck != null && (new Vector3(target.Position.X, target.Position.Y, target.Position.Z)).DistanceTo(LSCustom.DepotVehicle) <= 10)
            {
                xmenu.Add(new XMenuItem("Transfert Atelier", "", "ID_atelier", XMenuItemIcons.WRENCH_SOLID, true));
            }

            var playerHandler = client.GetPlayerHandler();

            if (playerHandler == null)
                return await base.InteractVehicleMenu(client, target, xmenu);

            if (playerHandler.GetStacksItems(ItemID.CrateTool).Count > 0)
            {
                var _crateToolItem = new XMenuItem("Réparer", "Réparer avec la caisse a outils", "", XMenuItemIcons.TOOLBOX_SOLID, false);
                _crateToolItem.OnMenuItemCallback = UseCrateTool;
                xmenu.Add(_crateToolItem);
            }

            return await base.InteractVehicleMenu(client, target, xmenu);
        }

        private async Task MenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IVehicle veh = menu.GetData("Vehicle");
            if (veh == null)
                return;

            var vh = veh.GetVehicleHandler();

            if (vh == null)
                return;

            switch (menuItem.Id)
            {
                case "ID_attach":
                    if (vh.TowTruck == null)
                    {
                        VehicleHandler _vh = (new Position(client.Position.X, client.Position.Y, client.Position.Z).GetTowTruckInZone(5)?.GetVehicleHandler());
                        if (_vh != null)
                        {
                            await _vh.TowVehicle(veh);
                        }
                        else
                        {
                            client.SendNotificationError("Aucune dépanneuse dans les environs");
                        }
                        await XMenuManager.XMenuManager.CloseMenu(client);
                    }
                    break;
                case "ID_detach":
                    var rot = await veh.GetRotationAsync();
                    await vh.UnTowVehicle(new Location((new Vector3(client.Position.X, client.Position.Y, client.Position.Z)).Forward(rot.Yaw, -10), rot));
                    vh.UpdateFull();
                    await XMenuManager.XMenuManager.CloseMenu(client);
                    break;
                case "ID_atelier":
                    await vh.UnTowVehicle(LSCustom.ReparZoneVL);
                    vh.UpdateFull();

                    await XMenuManager.XMenuManager.CloseMenu(client);
                    break;
            }
        }
        #endregion
    }
}
