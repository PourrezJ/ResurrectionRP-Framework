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

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        private PlayerHandler PlayerHandler;
        public async Task OpenXtremMenu(IPlayer client)
        {
            PlayerHandler = client.GetPlayerHandler();

            if (PlayerHandler == null || Vehicle == null) return;

            XMenu xmenu = new XMenu("VehiculeMenu");
            xmenu.Callback = VehicleXMenuCallback;

            var locked = await Vehicle.GetLockStateAsync();

            if (await client.IsInVehicleAsync())
            {
                xmenu.Add((locked == VehicleLockState.Locked ? new XMenuItem("Déverrouiller", "Déverrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_OPEN_SOLID, false)
                     : new XMenuItem("Verrouiller", "Verrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_SOLID, false)));

                if (await client.IsInVehicleAsync() && await client.GetSeatAsync() == 1)
                {
                    xmenu.Add(new XMenuItem($"{(client.Vehicle.EngineOn ? "Eteindre" : "Allumer")} le véhicule", "", "ID_start", XMenuItemIcons.KEY_SOLID, executeCallback: true));
                    
                    if (locked == VehicleLockState.Unlocked)
                        xmenu.Add(new XMenuItem("Gestion des portes", "", "ID_doors", XMenuItemIcons.DOOR_CLOSED_SOLID, executeCallback: true));
                        
                    if (NeonsColor != Color.Empty && (VehicleManifest?.Neon == true))
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
                {
                    xmenu.Add(new XMenuItem("Louer", $"Louer le véhicule pour le prix de ${ data.VehicleInfo.Price}", "ID_Rent", XMenuItemIcons.MONEY_BILL_SOLID, false));
                }
                else
                {
                    if (PlayerHandler.ListVehicleKey.Exists(key => key.Plate == Plate))
                    {
                        xmenu.Add((locked == VehicleLockState.Locked ? new XMenuItem("Déverrouiller", "Déverrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_OPEN_SOLID, false)
                            : new XMenuItem("Verrouiller", "Vérrouille le véhicule", "ID_LockUnlockVehicle", XMenuItemIcons.LOCK_SOLID, false)));
                    }
                }
            }

            if ((locked == VehicleLockState.Unlocked || PlayerHandler.ListVehicleKey.Exists(key => key.Plate == Plate))/* && Inventory != null*/)
            {
                xmenu.Add(new XMenuItem("Inventaire", "Ouvre l'inventaire du véhicule", "ID_OpenInventory", XMenuItemIcons.SUITCASE_SOLID, false));
            }

            if (OwnerID == client.GetSocialClub() && !SpawnVeh)
            {
                xmenu.Add(new XMenuItem("Donner le véhicule", "", "ID_give", XMenuItemIcons.HAND_HOLDING_SOLID, true));
            }

            if (PlayerHandler.StaffRank >= AdminRank.Mapper)
            {
                xmenu.Add(new XMenuItem("Supprimer le véhicule PERM", "", "ID_delete", XMenuItemIcons.DELETE, true));
            }
            /*
           var lockPicks = PlayerHandler.GetStacksItems(ItemID.LockPick);
           if (lockPicks.Count > 0)
           {
               xmenu.Add(new XMenuItem("Crocheter le véhicule", "", "ID_lockpick", XMenuItemIcons.SCREWDRIVER_SOLID, true));
           }

           if (await FactionManager.IsLSCustom(client) || await FactionManager.IsLspd(client) || await FactionManager.IsMedic(client) || await FactionManager.IsRebelle(client))
           {
               xmenu.Add(new XMenuItem("Faction", "", "ID_Faction", XMenuItemIcons.ID_BADGE_SOLID, false));
           }
           */
            await xmenu.OpenXMenu(client);
        }

        private async Task OpenDoorsMenu(IPlayer client)
        {
            XMenu menu = new XMenu("DoorMenu");
            menu.Callback = VehicleXMenuCallback;
            
            menu.Add(new XMenuItem("Porte avant gauche", "", "ID_frontLeft",  GetXMenuIconDoor( this.GetDoorState(VehicleDoor.DriverFront) ) )    );
            menu.Add(new XMenuItem("Porte avant droite", "", "ID_frontRight", GetXMenuIconDoor(this.GetDoorState(VehicleDoor.PassengerFront))));
            menu.Add(new XMenuItem("Porte arrière gauche", "", "ID_backLeft", GetXMenuIconDoor(this.GetDoorState(VehicleDoor.DriverRear))));
            menu.Add(new XMenuItem("Porte arrière droite", "", "ID_backRight", GetXMenuIconDoor(this.GetDoorState(VehicleDoor.PassengerRear))));
            menu.Add(new XMenuItem("Capot", "", "ID_hood", GetXMenuIconDoor(this.GetDoorState(VehicleDoor.Hood))));
            menu.Add(new XMenuItem("Coffre", "", "ID_trunk", GetXMenuIconDoor(this.GetDoorState(VehicleDoor.Trunk) )));
            
            await menu.OpenXMenu(client);
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
                    await LockUnlock(client);
                    await OpenXtremMenu(client);
                    break;
                /*
                    case "ID_OpenInventory":

                        if (RPGInventoryManager.HasInventoryOpen(Inventory))
                        {
                            await client.SendNotificationError("Le coffre est déjà occupé.");
                            return;
                        }

                        await XMenuManager.CloseMenu(client);
                        var inv = new RPGInventoryMenu(PlayerHandler.PocketInventory, PlayerHandler.OutfitInventory, PlayerHandler.BagInventory, this.Inventory);
                        inv.OnOpen = ((IPlayer c, RPGInventoryMenu m) =>
                        {
                            Inventory.Locked = true;
                            return Task.CompletedTask;
                        });
                        inv.OnMove = (async (IPlayer c, RPGInventoryMenu m) =>
                        {
                            if (PlayerHandler != null)
                            {
                                await PlayerHandler.UpdatePlayerInfo();
                            }

                            await this.Update();
                        });
                        inv.OnClose = ((IPlayer c, RPGInventoryMenu m) =>
                        {
                            Inventory.Locked = false;
                            return Task.CompletedTask;
                        });
                        await inv.OpenMenu(client);
                        break;
                            */
                    #warning DOOR SYSTEM DESACTIVER SONT COMPORTEMENT EST ETRANGE SUR ALTV
                    
                case "ID_doors":
                    await OpenDoorsMenu(client);
                    break;

                case "ID_frontLeft":
                    await SetDoorState(VehicleDoor.DriverFront, (this.GetDoorState(VehicleDoor.DriverFront) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    await OpenDoorsMenu(client);
                    break;
                case "ID_frontRight":
                    await SetDoorState(VehicleDoor.PassengerFront, (this.GetDoorState(VehicleDoor.PassengerFront) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    await OpenDoorsMenu(client);
                    break;
                case "ID_backLeft":
                    await SetDoorState(VehicleDoor.DriverRear, (this.GetDoorState(VehicleDoor.DriverRear) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    await OpenDoorsMenu(client);
                    break;
                case "ID_backRight":
                    await SetDoorState(VehicleDoor.PassengerRear, (this.GetDoorState(VehicleDoor.PassengerRear) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    await OpenDoorsMenu(client);
                    break;
                case "ID_hood":
                    await SetDoorState(VehicleDoor.Hood, (this.GetDoorState(VehicleDoor.Hood) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    await OpenDoorsMenu(client);
                    break;
                case "ID_trunk":
                    await SetDoorState(VehicleDoor.Trunk, (this.GetDoorState(VehicleDoor.Trunk) >= VehicleDoorState.OpenedLevel1 ? VehicleDoorState.Closed : VehicleDoorState.OpenedLevel7));
                    await OpenDoorsMenu(client);
                    break;
                    /*
                case "ID_neons":
                    if (VehicleSync.NeonsColor == null)
                    {
                        await client.SendNotificationError("Aucun néons d'installer sur la voiture.");
                        return;
                    }

                    VehicleSync.NeonState = !VehicleSync.NeonState;

                    await Vehicle.SetNeonsColorAsync(VehicleSync.NeonsColor);

                    await Vehicle.SetNeonsActiveAsync(VehicleSync.NeonState);
                    await XMenuManager.CloseMenu(client);
                    break;
                    */
                    case "ID_start":
                        var engineCurrent = await Vehicle.IsEngineOnAsync();
                        await Vehicle.SetEngineOnAsync(!engineCurrent);
                        await XMenuManager.XMenuManager.CloseMenu(client);
                        break;
                        /*
                    case "ID_give":
                        List<PlayerHandler> players = await PlayerManager.GetNearestPlayers(client);
                        if (players.Count > 0)
                        {
                            Menu menugive = new Menu("ID_GiveVehicle", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);

                            foreach (PlayerHandler player in players)
                            {
                                MenuItem pitem = new MenuItem(player.Identite.Name, executeCallback: true, id: "Give");
                                menugive.Add(pitem);
                            }

                            menugive.Callback = (async (_client, _menu, _menuitem, _menuindex, _data) =>
                            {
                                PlayerHandler destinataire = PlayerManager.GetPlayerByName(_menuitem.Text);
                                if (destinataire != null)
                                {
                                    await SetOwner(destinataire);
                                    var vehinfo = VehicleInfoLoader.VehicleInfoLoader.Get(await Vehicle.GetModelAsync());
                                    await _client.SendNotificationSuccess("Vous avez donné votre " + vehinfo.LocalizedManufacturer + " " + vehinfo.LocalizedName + " à " + destinataire.Identite.Name);
                                    await destinataire.Client.SendNotificationSuccess("Vous avez reçu un(e) " + vehinfo.LocalizedManufacturer + " " + vehinfo.LocalizedName + " par " + PlayerManager.GetPlayerByClient(_client)?.Identite.Name);

                                    await this.Update();
                                    await MenuManager.CloseMenu(_client);
                                }
                            });

                            await MenuManager.OpenMenu(client, menugive);
                        }
                        else
                        {
                            await client.SendNotificationError("Personne autour de vous.");
                            await XMenuManager.CloseMenu(client);
                        }
                        break;
                        */
                    case "ID_Buy":
                        try
                        {
                            await XMenuManager.XMenuManager.CloseMenu(client);
                            int carPrice;
                            if (Vehicle.TryGetData("CarDealer", out dynamic _data) == true)
                            {
                                Vehicle.TryGetData("CarDealerPrice", out carPrice);
                                Loader.CarDealerLoader.CarDealerPlace _place = _data;
                                PlayerHandler ph = client.GetPlayerHandler();
                                if (ph != null)
                                {
                                    if (await ph.HasBankMoney(carPrice, $"Achat de véhicule {_place.VehicleInfo.Name} {_place.VehicleHandler.Plate}."))
                                    {
                                        await _place.CarDealer.BuyCar(_place, ph);
                                    }
                                    else
                                    {
                                        client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque");
                                    }
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
                            await XMenuManager.XMenuManager.CloseMenu(client);

                            if (Vehicle.TryGetData("RentShop", out dynamic _data) == true)
                            {
                                Loader.VehicleRentLoader.VehicleRentPlace _place = _data;
                                PlayerHandler ph = client.GetPlayerHandler();
                                if (ph != null)
                                {
                                    if (await ph.HasBankMoney(_place.VehicleInfo.Price, $"Location de véhicule {_place.VehicleInfo.Name} {_place.VehicleHandler.Plate}."))
                                    {
                                        _place.RentShop.RentCar(_place, ph);
                                    }
                                    else
                                    {
                                        client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Alt.Server.LogError("VehicleHandler.Menu.cs : " + ex.ToString() );
                        }
                        break;
                    
                    case "ID_delete":
                        await this.Delete(true);
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


                    case "ID_Faction":
                        menu.ClearItems();
                        FactionManager.AddFactionVehicleMenu(client, Vehicle, menu);
                        await menu.OpenXMenu(client);

                        break;

                    default:
                        break;
                }*/
            }
            #endregion
        }
    }
}