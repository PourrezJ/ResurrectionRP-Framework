using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Colshape;
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
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class LSCustom : Faction
    {
        #region Fields and properties
        public static Vector3 DepotVehicle = new Vector3(-357.0305f, -134.4201f, 38.93036f);
        public static Location ReparZoneVL = new Location(new Vector3(-325.1813f, -134.0631f, 38.5204f), new Vector3(-0.992146f, -1.029223f, 306.1207f));

        public static Vector3 PeintureZone = new Vector3(-326.7145f, -144.8862f, 37.55893f);

        private static readonly int ReparEnginePrice = 900;
        private static readonly int ReparBody = 700;
        private static readonly int ReparFortune = 1000;
        private static readonly int PeinturePrice = 2000;
        private static readonly int ClearVehicle = 0;

        private static bool onReparation;

        [BsonIgnore]
        private IVehicle VehicleInWorkbench;

        [BsonIgnore]
        private IVehicle VehicleInColorCabin;

        [BsonIgnore]
        public IColshape ReparationVLColshape { get; private set; }
        [BsonIgnore]
        public IColshape PeintureColshape { get; private set; }
        #endregion

        #region Constructor
        public LSCustom(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }
        #endregion

        #region Init
        public override Faction Init()
        {
            Vector3 reparZone = new Vector3(-324.7894f, -134.2555f, 37.54341f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, reparZone, new Vector3(4, 4, 1), Color.FromArgb(80,255,255,255), GameMode.GlobalDimension);
            ReparationVLColshape = ColshapeManager.CreateCylinderColshape(reparZone, 4, 4);
            ReparationVLColshape.OnPlayerInteractInColshape += OnEnterRepairZoneVL;
            ReparationVLColshape.OnPlayerEnterColshape += OnEnterColshapeInteract;
            ReparationVLColshape.OnVehicleEnterColshape += OnVehicleEnterColshape;
            ReparationVLColshape.OnVehicleLeaveColshape += OnVehicleLeaveColshape;

            PeintureColshape = ColshapeManager.CreateCylinderColshape(PeintureZone, 4, 4);
            Marker.CreateMarker(MarkerType.VerticalCylinder, PeintureZone, new Vector3(4, 4, 1), Color.FromArgb(80, 255, 255, 255), GameMode.GlobalDimension);
            PeintureColshape.OnPlayerInteractInColshape += OnEnterPaintZoneVL;
            PeintureColshape.OnPlayerEnterColshape += OnEnterColshapeInteract;
            PeintureColshape.OnVehicleEnterColshape += OnVehicleEnterColshape;
            PeintureColshape.OnVehicleLeaveColshape += OnVehicleLeaveColshape;

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
            ServiceLocation = new Vector3(-348.72528f, -121.76703f, 39f);
            ShopLocation = new Vector3(-350.74286f, -125.74945f, 38.002197f);

            // Items SHOP
            // ItemShop.Add(new FactionShopItem(new LockPick(ItemID.LockPick, "Kit De Crochetage", "", 0.2, true, false, true, true), 5000, 1));
            ItemShop.Add(new FactionShopItem(new CrateTools(ItemID.CrateTool, "Caisse a outil", "De marque Facom", 1, true, false, true, true), 15000, 1));

            return base.Init();
        }
        #endregion

        #region Event handlers
        private void OnEnterPaintZoneVL(IColshape colShape, IPlayer client)
        {
            if (VehicleInColorCabin == null)
            {
                client.SendNotificationError("Le véhicule doit être dans la cabine de peinture pour être repeint.");
                return;
            }

            ICollection<string> employees = GetAllSocialClubName();
            string social = client.GetSocialClub();

            if (employees.Contains(social))
                OpenPeintureMenu(client);
        }

        private void OnVehicleLeaveColshape(IColshape colShape, IVehicle vehicle)
        {
            if (colShape == ReparationVLColshape)
                VehicleInWorkbench = null;
            else if (colShape == PeintureColshape)
                VehicleInColorCabin = null;

            if (vehicle.Driver != null)
                MenuManager.CloseMenu(vehicle.Driver);
        }

        private void OnVehicleEnterColshape(IColshape colShape, IVehicle vehicle)
        {
            if (colShape == ReparationVLColshape)
                VehicleInWorkbench = vehicle;
            else if (colShape == PeintureColshape)
                VehicleInColorCabin = vehicle;
        }

        private void OnEnterColshapeInteract(IColshape colShape, IPlayer client)
        {
            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
        }

        private void OnPeintureSelect(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
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

                if (menuItem.Id == "ID_First" && (byte)color == VehicleInColorCabin.PrimaryColor || menuItem.Id == "ID_Second" && (byte)color == VehicleInColorCabin.SecondaryColor || menuItem.Id == "ID_Pearl" && (byte)color == VehicleInColorCabin.PearlColor)
                    item.RightBadge = BadgeStyle.Makeup;
                else
                {
                    item.RightLabel = $"${PeinturePrice}";
                    item.OnMenuItemCallback = OnColorChoice;
                }

                item.SetData("Color", Convert.ToInt32(color));
                menu.Add(item);
            }

            menu.OpenMenu(client);
            PeinturePreview(client, menu, 0, menu.Items[0]);
        }

        private void PeintureSelectCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null && VehicleInColorCabin != null)
            {
                VehicleHandler vh = VehicleInColorCabin.GetVehicleHandler();
                VehicleInColorCabin.PrimaryColor = vh.PrimaryColor;
                VehicleInColorCabin.SecondaryColor = vh.SecondaryColor;
                VehicleInColorCabin.PearlColor = vh.PearlColor;
                OpenPeintureMenu(client);
            }
        }

        private void OnColorChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
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

            if (BankAccount.GetBankMoney(PeinturePrice, $"Peinture du véhicule {vh.VehicleData.Plate} par {ph.Identite.Name}"))
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

                vh.UpdateInBackground(false);
                client.SendNotificationSuccess("Peinture effectuée!");
                OpenPeintureMenu(client);
            }
            else
                client.SendNotificationError("Le fond de commerce est vide.");
        }

        private void OnEnterRepairZoneVL(IColshape colShape, IPlayer client)
        {
            if (VehicleInWorkbench != null)
                OpenMenu(client, VehicleInWorkbench);
        }

        public override void OnPlayerServiceEnter(IPlayer client, int rang)
        {
            switch (client.GetPlayerHandler().Character.Gender)
            {
                case Sex.Men: // Homme
                    client.SetCloth(ClothSlot.Undershirt, 8, 36, 0);
                    break;
                case Sex.Female: // Femme
                    client.SetCloth(ClothSlot.Undershirt, 8, 36, 0);
                    break;
                default: // Ped?
                    client.SendNotificationError("Vous ne pouvez pas avoir de tenue avec ce personnage");
                    break;
            }

            base.OnPlayerServiceEnter(client, rang);
        }

        public override void OnPlayerServiceQuit(IPlayer client, int rang)
        {
            // rendre la tenue
            client.ApplyCharacter();
            base.OnPlayerServiceQuit(client, rang);
        }
        #endregion

        #region Methods
        public void OpenPeintureMenu(IPlayer client)
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

            menu.OpenMenu(client);
        }

        private void PeinturePreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            int color = menuItem.GetData("Color");

            if (VehicleInColorCabin == null)
                return;

            if (menu.Id == "ID_First")
                VehicleInColorCabin.PrimaryColor = (byte)color;
            else if (menu.Id == "ID_Second")
                VehicleInColorCabin.SecondaryColor = (byte)color;
            else if (menu.Id == "ID_Pearl")
                VehicleInColorCabin.PearlColor = (byte)color;
        }
        #endregion

        #region Menus
        public override XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            var playerHandler = client.GetPlayerHandler();
            xmenu.SetData("Player", target);

            return base.InteractPlayerMenu(client, target, xmenu);
        }

        private void UseCrateTool(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
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
                    CrateTools.RepairVehicle(client, vehicle);
                }
            }
        }

        public void OpenMenu(IPlayer client, IVehicle vehicle)
        {
            if (onReparation)
            {
                client.SendNotificationError("Une réparation est déjà en cours.");
                return;
            }

            Menu menu = new Menu("ID_MainReparMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.CarMod);
            menu.SetData("Vehicle", vehicle);
            menu.ItemSelectCallback = MenuCallBack;

            if (HasPlayerIntoFaction(client) || FactionPlayerList.Count == 0)
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

            menu.OpenMenu(client);
        }

        private void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            IVehicle veh = menu.GetData("Vehicle");

            if (veh == null|| !veh.Exists)
                return;

            PlayerHandler ph = client.GetPlayerHandler();
            VehicleHandler _vh = veh.GetVehicleHandler();
            onReparation = true;

            switch (menuItem.Id)
            {
                case "ID_Diag":
                    client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Diagnostique: ~r~Démarrage~w~.", "En cours ...");

                    Utils.Utils.Delay(20000, () =>
                    {
                        AltAsync.Do(() =>
                        {
                            if (!veh.Exists || !client.Exists)
                                return;

                            string str =
                            "~r~Résultat:~w~\n" +
                            $"Chassis:   {Math.Floor(veh.BodyHealth * 0.1)}% \n" +
                            $"Moteur:    {Math.Floor(veh.EngineHealth * 0.1)}%\n" +
                            $"Réservoir: {Math.Floor(veh.PetrolTankHealth * 0.1)}%\n";

                            client.DisplaySubtitle(str, 5000);
                            onReparation = false;
                        });
                    });
                    menu.CloseMenu(client);
                    break;

                case "ID_Body":
                    if (BankAccount.GetBankMoney(ReparBody, $"Réparation carrosserie {_vh.VehicleData.Plate} par {ph.Identite.Name}"))
                    {
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Carrosserie: ~r~Démarrage~w~.", "En cours ...");
                        _vh.VehicleData.UpdateProperties();

                        Utils.Utils.Delay(20000, () =>
                        {
                            AltAsync.Do(() =>
                            {
                                if (client == null || !client.Exists)
                                    return;
                                int engineHealth = _vh.EngineHealth;
                                int petrolTankHealth = _vh.PetrolTankHealth;
                                _vh.Repair(client);
                                _vh.EngineHealth = engineHealth;
                                _vh.PetrolTankHealth = petrolTankHealth;
                                _vh.UpdateInBackground(false);
                                var vehdata = _vh.VehicleData;
                                _vh.Remove();
                                vehdata.SpawnVehicle();
                                client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Carrosserie: ~g~Terminé~w~.", "Elle est niquel!");
                                onReparation = false;
                                UpdateInBackground();
                            });
                        });
                        menu.CloseMenu(client);
                       
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");

                    break;

                case "ID_Engine":
                    if (BankAccount.GetBankMoney(ReparEnginePrice, $"Réparation moteur {_vh.VehicleData.Plate} par {ph.Identite.Name}"))
                    {
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~r~Démarrage~w~.","C'est parti!");

                        Utils.Utils.Delay(20000, () =>
                        {
                            AltAsync.Do(() =>
                            {
                                if (!veh.Exists || !client.Exists)
                                    return;

                                client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~g~Terminé~w~.", "Il est niquel!");

                                _vh.EngineHealth = 1000;
                                _vh.VehicleData.EngineHealth = 1000;
                                _vh.UpdateInBackground(false);
                                onReparation = false;
                                UpdateInBackground();
                            });
                        });
                        menu.CloseMenu(client);
                    }
                    else
                       client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");

                    break;

                case "ID_Clean":
                    if (BankAccount.GetBankMoney(ClearVehicle, $"Néttoyage vehicule {_vh.VehicleData.Plate} par {ph.Identite.Name}"))
                    {
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Nettoyage: ~r~Démarrage~w~.","C'est parti!" );

                        Utils.Utils.Delay(20000, () =>
                        {
                            AltAsync.Do(() => {
                                if (!veh.Exists || !client.Exists)
                                    return;

                                client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Nettoyage: ~g~Terminé~w~.", "Elle est niquel!");
                                _vh.DirtLevel = 0;
                                veh.DirtLevel = 0;
                                _vh.UpdateInBackground(false);
                                _vh.ApplyDamage();
                                onReparation = false;
                                UpdateInBackground();
                            });
                        });
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");
                    menu.CloseMenu(client);
                    break;

                case "ID_BricoEngine":
                    if (client.GetPlayerHandler().HasMoney(ReparFortune))
                    {
                        client.SendNotificationPicture( CharPicture.CHAR_LS_CUSTOMS ,"Los Santos Custom", "Réparation Moteur: ~r~Démarrage~w~.", "Alors ce tuyau va où déjà?");

                        Utils.Utils.Delay(20000, () =>
                        {
                            AltAsync.Do(() => {
                                if (!veh.Exists || !client.Exists)
                                    return;

                                client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~g~Terminé~w~.", "Le moteur démarre, c'est déjà ça!");
                                _vh.EngineHealth = 400;
                                _vh.UpdateInBackground(false);
                                _vh.ApplyDamage();
                                onReparation = false;
                                UpdateInBackground();
                            });
                        });
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent sur vous!");
                    menu.CloseMenu(client);
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

        public static bool IsWhitelistClassTow(IVehicle vehicle)
        {
            int[] whitelistClass = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 18 };

            var data = VehicleInfoLoader.VehicleInfoLoader.Get(vehicle.Model);
            if (data != null)
            {
                if ( (data.VehicleClass == 18 || data.VehicleClass <= 12 && data.VehicleClass >= 0) && data.VehicleClass <= 18)
                    return true;
            }
            return false;
        }

        public override XMenu InteractVehicleMenu(IPlayer client, IVehicle target, XMenu xmenu)
        {
            xmenu.SetData("Vehicle", target);

            xmenu.Callback += MenuCallback;
            var nearest = client.GetNearestVehicle(10);

            if (target.Model != (int)VehicleModel.Flatbed && LSCustom.IsWhitelistClassTow(target) == true)
            {
                xmenu.Add(new XMenuItem("Remorquer", "", "ID_attach", XMenuItemIcons.TRUCK_LOADING_SOLID, true));
            }

            if (target.Model == (int)VehicleModel.Flatbed && target.GetVehicleHandler().VehicleData.TowTruck != null)
            {
                xmenu.Add(new XMenuItem("Détacher", "", "ID_detach", XMenuItemIcons.TRUCK_LOADING_SOLID, true));
            }

            if (FactionManager.IsLSCustom(client) && target.Model == (uint)VehicleModel.Flatbed && target.GetVehicleHandler().VehicleData.TowTruck != null && (new Vector3(target.Position.X, target.Position.Y, target.Position.Z)).DistanceTo(LSCustom.DepotVehicle) <= 10)
            {
                xmenu.Add(new XMenuItem("Transfert Atelier", "", "ID_atelier", XMenuItemIcons.WRENCH_SOLID, true));
            }

            var playerHandler = client.GetPlayerHandler();

            if (playerHandler == null)
                return base.InteractVehicleMenu(client, target, xmenu);

            if (playerHandler.GetStacksItems(ItemID.CrateTool).Count > 0)
            {
                var _crateToolItem = new XMenuItem("Réparer", "Réparer avec la caisse a outils", "", XMenuItemIcons.TOOLBOX_SOLID, false);
                _crateToolItem.OnMenuItemCallback = UseCrateTool;
                xmenu.Add(_crateToolItem);
            }

            return base.InteractVehicleMenu(client, target, xmenu);
        }

        private void MenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
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
                    XMenuManager.XMenuManager.CloseMenu(client);
                    if (vh.VehicleData.TowTruck == null)
                    {
                        IVehicle towtruck = null;
                        foreach(IVehicle _vh in veh.GetVehiclesInRange(20))
                        {
                            if (_vh.Model == (uint)VehicleModel.Flatbed)
                                towtruck = _vh;
                            
                        }

                        if (towtruck != null)
                            towtruck.GetVehicleHandler().TowVehicle(veh);
                        else
                            client.SendNotificationError("Aucune dépanneuse dans les environs");


                        XMenuManager.XMenuManager.CloseMenu(client);
                    }
                    break;
                case "ID_detach":
                    vh.UnTowVehicle(new Location((new Vector3(client.Position.X, client.Position.Y, client.Position.Z)).Forward(veh.Rotation.Yaw, -10), veh.Rotation));
                    vh.UpdateInBackground(false);
                    XMenuManager.XMenuManager.CloseMenu(client);
                    break;
                case "ID_atelier":
                    vh.UnTowVehicle(ReparZoneVL);
                    vh.UpdateInBackground(false);

                    XMenuManager.XMenuManager.CloseMenu(client);
                    break;
            }
        }
        #endregion
    }
}
