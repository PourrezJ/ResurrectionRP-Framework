using AltV.Net.Async;
using AltV.Net.Async.Events;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Utils.Extensions;
using ResurrectionRP_Server.XMenuManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        private PlayerHandler PlayerHandler;
        public void OpenXtremMenu(IPlayer client)
        {
            PlayerHandler = client.GetPlayerHandler();

            if (PlayerHandler == null || Vehicle == null)
                return;

            XMenu xmenu = new XMenu("VehiculeMenu");
            xmenu.CallbackAsync = VehicleXMenuCallback;

            if (client.IsInVehicle)
            {
                xmenu.Add(LockState == VehicleLockState.Locked ? new XMenuItem("Déverrouiller", "Déverrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_OPEN_SOLID, false)
                     : new XMenuItem("Verrouiller", "Verrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_SOLID, false));

                if (client.IsInVehicle && client.Seat == 1)
                {
                    xmenu.Add(new XMenuItem($"{(client.Vehicle.EngineOn ? "Eteindre" : "Allumer")} le véhicule", "", "ID_Start", XMenuItemIcons.KEY_SOLID, executeCallback: true));
                    
                    if (LockState == VehicleLockState.Unlocked)
                        xmenu.Add(new XMenuItem("Gestion des portes", "", "ID_Doors", XMenuItemIcons.DOOR_CLOSED_SOLID, executeCallback: true));
                        
                    if (NeonColor.IsEmpty != false)
                        xmenu.Add(new XMenuItem($"{(NeonState.Item1 ? "Eteindre" : "Allumer")} les neons", "", "ID_neons", XMenuItemIcons.LIGHTBULB_SOLID, executeCallback: true));
                }
            }
            else
            {
                dynamic data = null;
                int carPrice;
                
                if (Vehicle.TryGetData("CarDealer", out data))
                {
                    Vehicle.TryGetData("CarDealerPrice", out carPrice);
                    xmenu.Add(new XMenuItem("Acheter", $"Acheter le véhicule pour le prix de ${ carPrice }", "ID_Buy", XMenuItemIcons.MONEY_BILL_SOLID, false));
                }
                else if (Vehicle.GetData("RentShop", out data))
                    xmenu.Add(new XMenuItem("Louer", $"Louer le véhicule pour le prix de ${ data.VehicleInfo.Price}", "ID_Rent", XMenuItemIcons.MONEY_BILL_SOLID, false));
                else if (PlayerHandler.ListVehicleKey.Exists(key => key.Plate == Plate))
                    xmenu.Add(LockState == VehicleLockState.Locked ? new XMenuItem("Déverrouiller", "Déverrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_OPEN_SOLID, false)
                        : new XMenuItem("Verrouiller", "Vérrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_SOLID, false));
            }

            if (LockState == VehicleLockState.Unlocked || PlayerHandler.ListVehicleKey.Exists(key => key.Plate == Plate)/* && Inventory != null*/)
                xmenu.Add(new XMenuItem("Inventaire", "Ouvre l'inventaire du véhicule", "ID_OpenInventory", XMenuItemIcons.SUITCASE_SOLID, false));
            
            if(Vehicle.GetVehicleHandler().hasTrailer)
                xmenu.Add(new XMenuItem("Remorque", "Détacher la remorque", "ID_DetachTrailer", XMenuItemIcons.TRAIN_SOLID, false));

            if (OwnerID == client.GetSocialClub() && !SpawnVeh)
                xmenu.Add(new XMenuItem("Donner le véhicule", "", "ID_give", XMenuItemIcons.HAND_HOLDING_SOLID, true));

            if (PlayerHandler.StaffRank >= AdminRank.Helper)
                xmenu.Add(new XMenuItem("Supprimer le véhicule PERM", "", "ID_delete", XMenuItemIcons.DELETE, true));
            
            var lockPicks = PlayerHandler.GetStacksItems(ItemID.LockPick);

            if (lockPicks.Count > 0)
                xmenu.Add(new XMenuItem("Crocheter le véhicule", "", "ID_lockpick", XMenuItemIcons.SCREWDRIVER_SOLID, true));

            if (FactionManager.IsLSCustom(client) || FactionManager.IsLspd(client) || FactionManager.IsMedic(client) || FactionManager.IsNordiste(client))
                xmenu.Add(new XMenuItem("Faction", "", "ID_Faction", XMenuItemIcons.ID_BADGE_SOLID, false));


            if (Vehicle != null && Array.IndexOf(Farms.Petrol.allowedTrailers, (VehicleModel)Vehicle.Model) != -1)
                xmenu.Add(new XMenuItem("Voir le litrage de la citerne", "", "ID_citerne", XMenuItemIcons.GAS_PUMP_SOLID, true));


            xmenu.OpenXMenu(client);
        }

        private void OpenDoorsMenu(IPlayer client)
        {
            XMenu menu = new XMenu("DoorMenu");
            menu.CallbackAsync = VehicleXMenuCallback;
            
            menu.Add(new XMenuItem("Porte avant gauche", "", "ID_frontLeft",  GetXMenuIconDoor(GetDoorState(VehicleDoor.DriverFront))));
            menu.Add(new XMenuItem("Porte avant droite", "", "ID_frontRight", GetXMenuIconDoor(GetDoorState(VehicleDoor.PassengerFront))));
            menu.Add(new XMenuItem("Porte arrière gauche", "", "ID_backLeft", GetXMenuIconDoor(GetDoorState(VehicleDoor.DriverRear))));
            menu.Add(new XMenuItem("Porte arrière droite", "", "ID_backRight", GetXMenuIconDoor(GetDoorState(VehicleDoor.PassengerRear))));
            menu.Add(new XMenuItem("Capot", "", "ID_hood", GetXMenuIconDoor(GetDoorState(VehicleDoor.Hood))));
            menu.Add(new XMenuItem("Coffre", "", "ID_trunk", GetXMenuIconDoor(GetDoorState(VehicleDoor.Trunk))));
            
            menu.OpenXMenu(client);
        }

        public XMenuItemIconDesc GetXMenuIconDoor(VehicleDoorState state)
        {
            if (state == VehicleDoorState.Closed)
                return XMenuItemIcons.DOOR_CLOSED_SOLID;
            else if (state > VehicleDoorState.Closed && state <= VehicleDoorState.OpenedLevel7)
                return XMenuItemIcons.DOOR_OPEN_SOLID;
            else if (state == VehicleDoorState.DoesNotExists)
                return XMenuItemIcons.BROKEN_IMAGE;

            return XMenuItemIcons.DEVICE_UNKNOWN;
        }

        #region Callback

        private async Task VehicleXMenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            if (Vehicle == null)
                return;

            if (!Vehicle.Exists)
                return;

            switch (menuItem.Id)
            {
                
                case "ID_LockUnlockVehicle":
                    LockUnlock(client);
                    OpenXtremMenu(client);
                    break;
                
                case "ID_OpenInventory":
                    if (RPGInventoryManager.HasInventoryOpen(Inventory))
                    {
                        client.SendNotificationError("Le coffre est déjà occupé.");
                        return;
                    }

                    XMenuManager.XMenuManager.CloseMenu(client);
                    var inv = new RPGInventoryMenu(PlayerHandler.PocketInventory, PlayerHandler.OutfitInventory, PlayerHandler.BagInventory, this.Inventory);
                    inv.OnOpen = (IPlayer c, RPGInventoryMenu m) =>
                    {
                        Inventory.Locked = true;
                        return Task.CompletedTask;
                    };

                    inv.OnMove = (IPlayer c, RPGInventoryMenu m) =>
                    {
                        if (PlayerHandler != null)
                            PlayerHandler.UpdateFull();

                        UpdateInBackground();
                        return Task.CompletedTask;
                    };

                    inv.OnClose = (IPlayer c, RPGInventoryMenu m) =>
                    {
                        Inventory.Locked = false;
                        return Task.CompletedTask;
                    };
                    await inv.OpenMenu(client);
                    break;
                case "ID_DetachTrailer":
                    client.Emit("DetachTrailer");
                    break;

                case "ID_Doors":
                    OpenDoorsMenu(client);
                    break;

                case "ID_frontLeft":
                    SetDoorState(client, VehicleDoor.DriverFront, (this.GetDoorState(VehicleDoor.DriverFront) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    OpenDoorsMenu(client);
                    break;
                case "ID_frontRight":
                    SetDoorState(client, VehicleDoor.PassengerFront, (this.GetDoorState(VehicleDoor.PassengerFront) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    OpenDoorsMenu(client);
                    break;
                case "ID_backLeft":
                    SetDoorState(client, VehicleDoor.DriverRear, (this.GetDoorState(VehicleDoor.DriverRear) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    OpenDoorsMenu(client);
                    break;
                case "ID_backRight":
                    SetDoorState(client, VehicleDoor.PassengerRear, (this.GetDoorState(VehicleDoor.PassengerRear) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    OpenDoorsMenu(client);
                    break;
                case "ID_hood":
                    SetDoorState(client, VehicleDoor.Hood, (this.GetDoorState(VehicleDoor.Hood) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    OpenDoorsMenu(client);
                    break;
                case "ID_trunk":
                    SetDoorState(client, VehicleDoor.Trunk, (this.GetDoorState(VehicleDoor.Trunk) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    OpenDoorsMenu(client);
                    break;
                    /*
                case "ID_neons":
                    if (VehicleSync.NeonsColor == null)
                    {
                        await client.SendNotificationError("Aucun néons d'installé sur la voiture.");
                        return;
                    }

                    VehicleSync.NeonState = !VehicleSync.NeonState;

                    await Vehicle.SetNeonsColorAsync(VehicleSync.NeonsColor);

                    await Vehicle.SetNeonsActiveAsync(VehicleSync.NeonState);
                    await XMenuManager.CloseMenu(client);
                    break;
                    */
                case "ID_Start":
                    EngineOn = !Vehicle.EngineOn;
                    UpdateInBackground();
                    XMenuManager.XMenuManager.CloseMenu(client);
                    break;
                        
                case "ID_give":
                    List<IPlayer> players = client.GetNearestPlayers(5f);

                    if (players.Count > 0)
                    {
                        Menu menugive = new Menu("ID_GiveVehicle", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);

                        foreach (IPlayer cliente in players)
                        {
                            PlayerHandler player = cliente.GetPlayerHandler();
                            MenuItem pitem = new MenuItem(player.Identite.Name, executeCallback: true, id: "Give");
                            menugive.Add(pitem);
                        }

                        menugive.ItemSelectCallbackAsync = async (_client, _menu, _menuitem, _menuindex) =>
                        {
                            PlayerHandler destinataire = PlayerManager.GetPlayerByName(_menuitem.Text);
                            if (destinataire != null)
                            {
                                SetOwner(destinataire);
                                var vehinfo = VehicleInfoLoader.VehicleInfoLoader.Get(await Vehicle.GetModelAsync());
                                _client.SendNotificationSuccess("Vous avez donné votre " + vehinfo.LocalizedManufacturer + " " + vehinfo.LocalizedName + " à " + destinataire.Identite.Name);
                                destinataire.Client.SendNotificationSuccess("Vous avez reçu un(e) " + vehinfo.LocalizedManufacturer + " " + vehinfo.LocalizedName + " par " + _client.GetPlayerHandler()?.Identite.Name);

                                UpdateInBackground();
                                MenuManager.CloseMenu(_client);
                            }
                        };

                        MenuManager.OpenMenu(client, menugive);
                    }
                    else
                    {
                        client.SendNotificationError("Personne autour de vous.");
                        XMenuManager.XMenuManager.CloseMenu(client);
                    }
                    break;
                        
                case "ID_Buy":
                    try
                    {
                        XMenuManager.XMenuManager.CloseMenu(client);
                        int carPrice;

                        if (Vehicle.TryGetData("CarDealer", out dynamic _data) == true)
                        {
                            Vehicle.TryGetData("CarDealerPrice", out carPrice);
                            Loader.CarDealerLoader.CarDealerPlace _place = _data;
                            PlayerHandler ph = client.GetPlayerHandler();

                            if (ph != null)
                            {
                                if (await ph.HasBankMoney(carPrice, $"Achat de véhicule {_place.VehicleInfo.Name} {_place.VehicleHandler.Plate}."))
                                    await _place.CarDealer.BuyCar(_place, ph);
                                else
                                    client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError("VehicleHandler.Menu.Cs : " + ex.ToString());
                    }
                    break;
                case "ID_Rent":
                    try
                    {
                        XMenuManager.XMenuManager.CloseMenu(client);

                        if (Vehicle.TryGetData("RentShop", out dynamic _data) == true)
                        {
                            Loader.VehicleRentLoader.VehicleRentPlace _place = _data;
                            PlayerHandler ph = client.GetPlayerHandler();

                            if (ph != null)
                            {
                                if (await ph.HasBankMoney(_place.VehicleInfo.Price, $"Location de véhicule {_place.VehicleInfo.Name} {_place.VehicleHandler.Plate}."))
                                    await _place.RentShop.RentCar(_place, ph);
                                else
                                    client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError("VehicleHandler.Menu.cs : " + ex.ToString() );
                    }
                    break;
                    
                case "ID_delete":
                    await this.DeleteAsync(true);
                    break;
                /*
                case "ID_lockpick":
                    var lockPicks = PlayerHandler.GetStacksItems(ItemID.LockPick);
                    if (lockPicks.Count > 0)
                    {
                        LockPick lockPick = null;
                        if (lockPicks.ContainsKey(InventoryTypes.Pocket))
                        {
                            lockPick = lockPicks[InventoryTypes.Pocket][0].Item as LockPick;
                            if (lockPick != null)
                                await LockPick.LockPickVehicle(client, Vehicle, client.GetPlayerHandler()?.PocketInventory);
                        }
                        else if (lockPicks.ContainsKey(InventoryTypes.Bag))
                        {
                            lockPick = lockPicks[InventoryTypes.Bag][0].Item as LockPick;
                            if (lockPick != null)
                                await LockPick.LockPickVehicle(client, Vehicle, client.GetPlayerHandler()?.BagInventory);
                        }
                    }
                    break;
                    */

                case "ID_Faction":
                    menu.ClearItems();
                    FactionManager.AddFactionVehicleMenu(client, Vehicle, menu);
                    menu.OpenXMenu(client);
                    break;

                case "ID_citerne":
                    client.DisplaySubtitle($"Contenu de la citerne: {OilTank.Traite}", 5000);
                    break;

                default:
                    break;
            }
            #endregion
        }
    }
}