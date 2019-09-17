﻿using AltV.Net;
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

        public static Vector3 PeintureZone = new Vector3(-326.7145f, -144.8862f, 38.55893f);

        private static readonly int ReparEnginePrice = 900;
        private static readonly int ReparBody = 700;
        private static readonly int ReparFortune = 1000;
        private static readonly int PeinturePrice = 2000;
        private static readonly int ClearVehicle = 0;

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
            Marker.CreateMarker(MarkerType.VerticalCylinder, new Vector3(-324.7894f, -134.2555f, 35.54341f), new Vector3(1, 1, 1));

            ReparationVLColshape = Alt.CreateColShapeCylinder(new Vector3(-324.7894f, -134.2555f, 39.54341f), 3, 1);
            PeintureColshape = Alt.CreateColShapeCylinder(PeintureZone, 3, 1);

            FactionRang = new FactionRang[] {
                new FactionRang(0,"Dépanneur", false, 2500, true),
                new FactionRang(1,"Bras Droit", false, 3000, true),
                new FactionRang(2,"Gérant", true, 3500, true, true)
            };

            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Flatbed, 230000));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.TowTruck, 150000));

            ServiceLocation = new Vector3(-324.7894f, -134.2555f, 35.54341f);
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

        public override async Task OnPlayerEnterColShape(IColShape colshapePointer, IPlayer player)
        {
            if (colshapePointer == ReparationVLColshape)
                await OnEnterReparZoneVL(colshapePointer, player);
            else if (colshapePointer == PeintureColshape)
                await OnEnterPeintureZone(colshapePointer, player);
        }

        public override async Task OnPlayerExitColShape(IColShape colshapePointer, IPlayer player)
        {
            if (colshapePointer != ReparationVLColshape && colshapePointer != PeintureColshape)
                return;

            var vehicle = await player.GetVehicleAsync();

            if (vehicle != null)
            {
                if (!vehicle.Exists)
                    return;

                VehicleHandler veh = vehicle.GetVehicleHandler();

                if (veh == null)
                    return;

                await vehicle.SetPrimaryColorAsync(veh.PrimaryColor);
                await vehicle.SetSecondaryColorAsync(veh.SecondaryColor);

                veh.Doors[(int)VehicleDoor.Hood] = VehicleDoorState.Closed;
                veh.Update();
            }
        }

        private async Task OnEnterPeintureZone(IColShape colShape, IPlayer client)
        {
            var employees = GetAllSocialClubName();
            var social =  client.GetSocialClub();

            if (!employees.Contains(social))
            {
                client.SendNotificationError("Vous n'êtes pas autorisé à être là.");
                return;
            }
            else if (employees.Contains(social) && !await client.IsInVehicleAsync())
            {
                client.SendNotificationError("Tu dois venir avec la voiture dans la chambre de peinture!");
                return;
            }
            else if (employees.Contains(social) && ! IsOnService(client))
            {
                client.SendNotificationError("Va te changer, tu ne vas pas peindre cette voiture comme ça!");
                return;
            }

            await OpenPeintureMenu(client);
        }

        private async Task OnPeintureSelect(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!await client.IsInVehicleAsync())
            {
                client.SendNotificationError("Tu dois rester dans le véhicule!");
                return;
            }

            menu = new Menu(menuItem.Id, "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.CarMod);
            menu.SetData("Vehicle", await client.GetVehicleAsync());
            menu.ItemSelectCallback = PeintureSelectCallback;
            menu.IndexChangeCallback = PeinturePreview;

            foreach (VehicleColor color in Enum.GetValues(typeof(VehicleColor)))
            {
                MenuItem item = new MenuItem(color.ToString(), executeCallback: true, executeCallbackIndexChange: true);
                item.RightLabel = $"${PeinturePrice}";
                item.SetData("Color", Convert.ToInt32(color));
                item.OnMenuItemCallback = OnColorChoise;
                menu.Add(item);
            }

            await menu.OpenMenu(client);
            await PeinturePreview(client, menu, 0, menu.Items[0]);
        }

        private async Task PeintureSelectCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                IVehicle vehicle = menu.GetData("Vehicle");
                VehicleHandler vh = vehicle.GetVehicleHandler();
                await vehicle.SetPrimaryColorAsync(vh.PrimaryColor);
                await vehicle.SetSecondaryColorAsync(vh.SecondaryColor);
                await OpenPeintureMenu(client);
            }
        }

        private async Task OnColorChoise(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            IVehicle vehicle = menu.GetData("Vehicle");
            int color = menuItem.GetData("Color");
            if (vehicle == null) return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            var vh = vehicle.GetVehicleHandler();

            if (vh == null)
                return;

            if (await BankAccount.GetBankMoney(PeinturePrice, $"Peinture du véhicule {vh.Plate} par {ph.Identite.Name}"))
            {
                if (menu.Id == "ID_First")
                {
                    await vehicle.SetPrimaryColorAsync((byte)color);
                    vh.PrimaryColor = (byte)color;
                }
                else if (menu.Id == "ID_Second")
                {
                    await vehicle.SetSecondaryColorAsync((byte)color);
                    vh.SecondaryColor = (byte)color;
                }

                vh.Update();
                client.SendNotificationSuccess("Peinture effectuée!");
            }
            else
            {
                client.SendNotificationError("Le fond de commerce est vide.");
            }
        }

        private async Task OnEnterReparZoneVL(IColShape colShape, IPlayer client)
        {
            if ( VehiclesManager.IsVehicleInSpawn(ReparZoneVL.Pos))
            {
                var vehs =  ReparZoneVL.Pos.GetVehiclesInRange(4f);

                if (vehs.Count > 0)
                {
                   await OpenMenu(client, vehs[0]);
                }
            }
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
        private async Task PeinturePreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            IVehicle vehicle = menu.GetData("Vehicle");
            int color = menuItem.GetData("Color");

            if (vehicle == null)
                return;

            var veh = vehicle.GetVehicleHandler();

            if (menu.Id == "ID_First")
                await vehicle.SetPrimaryColorAsync((byte)color);
            else if (menu.Id == "ID_Second")
                await vehicle.SetSecondaryColorAsync((byte)color);
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

            if ( HasPlayerIntoFaction(client))
            {
                menu.Add(new MenuItem("Diagnostique véhicule", "", "ID_Diag", true));
                menu.Add(new MenuItem("Réparer la carrosserie", "", "ID_Body", true));
                menu.Add(new MenuItem("Réparer le moteur", "", "ID_Engine", true));
                menu.Add(new MenuItem("Nettoyer le véhicule", "", "ID_Clean", true));
            }
            else
            {
                if (( GetEmployeeOnline()).Count > 0)
                    client.SendNotificationError("Vous devez passer par un mécanicien.");
                else
                    menu.Add(new MenuItem("Bricoler le moteur", "Faites une réparation à 50% des dégâts moteur.", "ID_BricoEngine", true, rightLabel: $"${ReparFortune}"));
            }

            await menu.OpenMenu(client);
        }

        public async Task OpenPeintureMenu(IPlayer client)
        {
            Menu menu = new Menu("ID_MainReparMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.CarMod);

            MenuItem primary = new MenuItem("Peinture principal", "", "ID_First", executeCallback: true, executeCallbackIndexChange: true);
            primary.OnMenuItemCallback = OnPeintureSelect;
            menu.Add(primary);
            MenuItem secondary = new MenuItem("Peinture secondaire", "", "ID_Second", executeCallback: true, executeCallbackIndexChange: true);
            secondary.OnMenuItemCallback = OnPeintureSelect;
            menu.Add(secondary);

            await menu.OpenMenu(client);
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            IVehicle veh = menu.GetData("Vehicle");
            if (veh == null) return;

            PlayerHandler ph = client.GetPlayerHandler();
            VehicleHandler _vh = veh.GetVehicleHandler();

            switch (menuItem.Id)
            {
                case "ID_Diag":
                    client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Diagnostique: ~r~Démarrage~w~.", "En cours ...");

                    await _vh.SetDoorState(VehicleDoor.Hood, VehicleDoorState.OpenedLevel7);
                    await veh.SetEngineOnAsync(true);

                    await client.PlayAnimation("mini@repair", "fixing_a_ped", 4, -8, -1, (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl));

                    Utils.Utils.Delay(20000, true, async () =>
                    {
                        if (!client.Exists)
                            return;

                        string str = $"Body: {Math.Floor(await veh.GetBodyHealthAsync() * 0.1)}% \n" +
                        $"Engine: {Math.Floor(await veh.GetEngineHealthAsync() * 0.1)} %";
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Diagnostique: ~g~Terminé~w~.", str);
                        await client.StopAnimationAsync();
                    });

                    break;

                case "ID_Body":
                    if (await BankAccount.GetBankMoney(ReparBody, $"Réparation carrosserie {_vh.Plate} par {ph.Identite.Name}"))
                    {
                        Utils.Utils.Delay(20000, true, async () =>
                        {
                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Carrosserie: ~g~Terminé~w~.", "Elle est niquel!");
                            await _vh.Vehicle.RepairAsync();
                            _vh.BodyHealth = 1000;
                            _vh.Doors = new VehicleDoorState[Globals.NB_VEHICLE_DOORS] { 0, 0, 0, 0, 0, 0 };
                            _vh.Windows = new WindowState[Globals.NB_VEHICLE_WINDOWS] { 0, 0, 0, 0 };

                            foreach (Wheel wheel in _vh.Wheels)
                            {
                                wheel.Health = 1000;
                                wheel.Burst = false;
                                wheel.HasTire = true;
                            }

                            _vh.Update();
                        });

                        await UpdateDatabase();
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Carrosserie: ~r~Démarrage~w~.", "C'est parti!");
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent dans les caisses!");

                    break;

                case "ID_Engine":
                    if (await BankAccount.GetBankMoney(ReparEnginePrice, $"Réparation moteur {_vh.Plate} par {ph.Identite.Name}"))
                    {
                        client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~r~Démarrage~w~.","C'est parti!");
                        var pos = await client.GetPositionAsync();

                        _vh.Freeze(true);
                        var bite = (new Vector3(client.Position.X, client.Position.Y, client.Position.Z)).Forward((await veh.GetRotationAsync()).Yaw, 2.2f);
                        await client.SetPositionAsync(bite);
                        await client.SetRotationAsync(await veh.GetRotationAsync());
                        //await client.PlayScenarioAsync("WORLD_HUMAN_VEHICLE_MECHANIC"); TODO

                        Utils.Utils.Delay(20000, true, async () =>
                        {
                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~g~Terminé~w~.","Il est niquel!");

                            _vh.EngineHealth = 1000;
                            _vh.Update();
                            _vh.UpdateProperties();
                            await client.SetPositionAsync(pos);
                            await client.StopAnimationAsync();
                            _vh.Freeze(false);
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

                        Utils.Utils.Delay(20000, true, () =>
                        {
                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Nettoyage: ~g~Terminé~w~.","Elle est niquel!");
                            _vh.Dirt = 0;
                            _vh.Update();
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

                        Utils.Utils.Delay(20000, true, () =>
                        {
                            client.SendNotificationPicture(CharPicture.CHAR_LS_CUSTOMS, "Los Santos Custom", "Réparation Moteur: ~g~Terminé~w~.","Le moteur démarre, c'est déjà ça!");
                            _vh.EngineHealth = 400;
                            _vh.Update();
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
                    vh.Update();
                    await XMenuManager.XMenuManager.CloseMenu(client);
                    break;
                case "ID_atelier":
                    await vh.UnTowVehicle(LSCustom.ReparZoneVL);
                    vh.Update();

                    await XMenuManager.XMenuManager.CloseMenu(client);
                    break;
            }
        }
        #endregion
    }
}
