using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using AltV.Net.Async;
using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.XMenuManager;

namespace ResurrectionRP_Server.Factions
{
    public partial class Faction
    {
        #region Fields
        private string _factionName;
        #endregion

        #region Menus
        #region TargetMenu
        public XMenu AddFactionTargetMenu(IPlayer client, IPlayer target, XMenu xMenu, XMenuItemIconDesc icon)
        {
            if ( HasPlayerIntoFaction(client))
            {
                var item = new XMenuItem(FactionName, "", "", icon, true);
                item.OnMenuItemCallback = AddFactionTargetMenu_Callback;
                xMenu.SetData("Faction_Target", target);
                xMenu.Add(item);
            }
            return xMenu;
        }

        private async Task AddFactionTargetMenu_Callback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            try
            {
                IPlayer target = menu.GetData("Faction_Target");
                if (target == null) return;

                menu = new XMenu("ID_Faction");
                InteractPlayerMenu(client, target, menu);
                await menu.OpenXMenu(client);
            }
            catch
            {
                // not castable, idk for what ??
            }
        }

        public virtual XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            xmenu.SetData("Player", target);
            if ( IsRecruteur(client))
            {
                if (FactionPlayerList.Keys.Contains( target.GetSocialClub()))
                {
                    var promote = new XMenuItem($"Changer le rang", "", "", XMenuItemIcons.HAND_PAPER_SOLID, true);
                    promote.OnMenuItemCallback = RankChangeChoise;
                    xmenu.Add(promote);
                }
                else
                {
                    var addfaction = new XMenuItem($"Ajouter {FactionName}", "", "", XMenuItemIcons.HAND_PAPER_SOLID, true);
                    addfaction.OnMenuItemCallback = InviteFactionChoise;
                    xmenu.Add(addfaction);
                }
            }

            return xmenu;
        }

        private async Task RankChangeChoise(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            if (_target == null) return;

            var clientRank =  GetRangPlayer(client);

            menu = new XMenu("");
            menu.SetData("Player", _target);

            var dismiss = new XMenuItem("Renvoyer", executeCallback: true);
            dismiss.OnMenuItemCallback = DissMissPlayer;
            menu.Add(dismiss);

            foreach (var rang in FactionRang)
            {
                if (rang.Rang > clientRank) continue;

                var item = new XMenuItem(rang.RangName, "", "", XMenuItemIcons.CHECK);
                item.OnMenuItemCallback = RankChange;
                item.SetData("Rang", rang);
                menu.Add(item);
            }

            await menu.OpenXMenu(client);
        }

        private async Task DissMissPlayer(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            if (_target == null) return;

            FactionPlayerList.Remove( _target.GetSocialClub(), out FactionPlayer value);
            _target.SendNotification($"Vous avez été renvoyé de {FactionName}.");
            await UpdateDatabase();
        }

        private async Task InviteFactionChoise(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            if (_target == null) return;

            var clientRank =  GetRangPlayer(client);

            menu = new XMenu("");
            menu.SetData("Player", _target);

            foreach (var rang in FactionRang)
            {
                if (rang.Rang > clientRank)
                    continue;

                var item = new XMenuItem(rang.RangName, "", "", XMenuItemIcons.CHECK);
                item.OnMenuItemCallback = InviteFaction;
                item.SetData("Rang", rang);
                menu.Add(item);
            }

            await menu.OpenXMenu(client);
        }

        private async Task InviteFaction(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            if (_target == null) return;
            FactionRang rang = menuItem.GetData("Rang");

            AcceptMenu accept = await AcceptMenu.OpenMenu(_target, $"{FactionName}", $"Rejoindre la faction {FactionName} rang {rang.RangName}?");
            accept.AcceptMenuCallBack = (async (IPlayer c, bool reponse) =>
            {
                if (reponse)
                {
                    if (await TryAddIntoFaction(_target, rang.Rang))
                    {
                        c.SendNotificationSuccess($"Vous faites désormais partie de la faction {FactionName} au rang {rang.RangName}.");
                        client.SendNotificationSuccess($"La personne fait dorénavant partie de {FactionName} au rang {rang.RangName}.");
                    }
                }
                else
                    client.SendNotificationError($"La personne ne souhaite pas rejoindre {FactionName}.");
            });
        }

        private async Task RankChange(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            if (_target == null) return;
            FactionRang rang = menuItem.GetData("Rang");
            var social =  _target.GetSocialClub();

            FactionPlayerList.GetOrAdd(social, new FactionPlayer(social, rang.Rang));

            int rangActuel = FactionPlayerList[social].Rang;

            FactionPlayerList[social].Rang = rang.Rang;
            await OnPlayerPromote(_target, itemIndex - 1);
            _target.SendNotification($"Vous êtes désormais {(rang.Rang >= rangActuel ? "~g~promu~w~" : "~r~rétrogradé~w~")} au rang de {rang.RangName}");
            client.SendNotificationSuccess($"Vous avez promu {_target.GetPlayerHandler().Identite.Name} au rang de {FactionRang[itemIndex - 1].RangName}");
            await UpdateDatabase();
        }
        #endregion

        #region VehicleMenu
        public XMenu AddFactionVehicleMenu(IPlayer client, IVehicle vehicle, XMenu xMenu, XMenuItemIconDesc icon)
        {
            if ( HasPlayerIntoFaction(client))
            {
                var item = new XMenuItem(FactionName, "", "", icon, true);
                item.OnMenuItemCallback = AddFactionVehicleMenu_Callback;
                xMenu.SetData("Faction_Target", vehicle);
                xMenu.Add(item);
            }
            return xMenu;
        }

        public virtual async Task AddFactionVehicleMenu_Callback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            try
            {
                IVehicle target = menu.GetData("Faction_Target");
                if (target == null || !target.Exists) return;

                menu = new XMenu("ID_Faction");
                await InteractVehicleMenu(client, target, menu);
                await menu.OpenXMenu(client);
            }
            catch
            {
                // not castable, idk for what ??
            }
        }

        public virtual Task<XMenu> InteractVehicleMenu(IPlayer client, IVehicle target, XMenu xmenu)
        {
            return Task.FromResult(xmenu);
        }
        #endregion

        public virtual async Task<Menu> PriseServiceMenu(IPlayer client)
        {
            if (HasPlayerIntoFaction(client))
            {
                Menu menu = new Menu("ID_ServiceMenu", this.FactionName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);

                MenuItem item = new MenuItem($"{(ServicePlayerList.Contains(client.GetSocialClub()) ? "Quitter" : "Prendre")} son service", "", "ID_PriseService", true);
                item.OnMenuItemCallback = ServiceMenuCallBack;
                menu.Add(item);

                var staffRank = client.GetPlayerHandler()?.StaffRank;

                // If is Owner ...
                if (CanTakeMoney(client) || staffRank >= AdminRank.Moderator)
                {
                    MenuItem getmoney = new MenuItem($"Gérer les finances", $"Caisse de la faction: ${BankAccount.Balance}", "ID_money", true);
                    getmoney.OnMenuItemCallback = FinanceMenu;
                    menu.Add(getmoney);
                }

                if (CanDepositMoney(client) || staffRank >= AdminRank.Moderator)
                {
                    MenuItem depot = new MenuItem("Déposer de l'argent dans les caisses", "", "ID_Depot", true);
                    depot.SetInput("", 10, InputType.UNumber, true);
                    depot.OnMenuItemCallback = DepotMoneyMenu;
                    menu.Add(depot);
                }

                if (IsRecruteur(client) || staffRank >= AdminRank.Moderator)
                {
                    MenuItem gestion = new MenuItem($"Gestion des membres", "", "ID_gestionMember", true);
                    gestion.OnMenuItemCallback = GestionMember;
                    menu.Add(gestion);
                }

                var vestiaire = new MenuItem("Ouvrir votre vestiaire", "", "ID_Vestiaire", true);
                vestiaire.OnMenuItemCallback = OpenVestiaire;
                menu.Add(vestiaire);

                var demission = new MenuItem("Démissioner", "", "ID_demissioner", executeCallback: true);
                demission.OnMenuItemCallback = GestionMemberCallback;
                demission.SetData("playerId", client.GetSocialClub());
                menu.Add(demission);
                await menu.OpenMenu(client);


                return menu;
            }
            else
                client.SendNotificationError("Vous ne faites pas partie de cette faction.");

            return null;
        }

        private async Task GestionMember(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.SubTitle = "Gestion des membres";
            menu.ItemSelectCallback = GestionMemberCallback;

            MenuItem ajouter = new MenuItem("Ajouter un employé", "", "add_employe", executeCallback: true);
            ajouter.Description = "Mettez le prénom puis le nom de famille pour l'ajouter.";
            ajouter.SetInput("Prénom Nom", 60, InputType.Text);
            menu.Add(ajouter);

            string ownerName = client.GetPlayerHandler()?.Identite.Name;

            if (FactionPlayerList.Count > 0)
            {
                foreach (var employe in FactionPlayerList)
                {
                    var identite = await Identite.GetOfflineIdentite(employe.Key);
                    menu.Add(new MenuItem(identite == null ? employe.Key : identite.Name, "", "delete_employe", executeCallback: true));
                }
            }

            await MenuManager.OpenMenu(client, menu);
        }

        private async Task GestionMemberCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await PriseServiceMenu(client);
                return;
            }

            if (menuItem.Id == "add_employe")
            {
                string _msg = menuItem.InputValue;
                if (!string.IsNullOrEmpty(_msg))
                {
                    var ph = PlayerManager.GetPlayerByName(_msg);

                    if (FactionPlayerList.Count >= 25)
                    {
                        client.SendNotificationError("Vous avez atteint le nombre maximun de membre.");
                        return;
                    }

                    if (ph != null)
                    {
                        if (FactionPlayerList.TryAdd(ph.PID, new FactionPlayer(ph.PID, 0)))
                        {
                            client.SendNotificationSuccess($"{_msg} est ajouté à la liste des employés");
                            await UpdateDatabase();
                        }
                    }
                    else
                    {
                        client.SendNotificationError($"{_msg} est introuvable.");
                    }
                }
                else
                {
                    client.SendNotificationError("Aucun nom de rentré.");
                }
                await PriseServiceMenu(client);
            }
            else if (menuItem.Id == "delete_employe")
            {
                foreach(var playerID in FactionPlayerList)
                {
                    PlayerHandler ph = PlayerManager.GetPlayerBySCN(playerID.Key);

                    if (ph != null && ph.Identite.Name == menuItem.Text)
                    {
                        if (ServicePlayerList.Contains(playerID.Key))
                            await PriseService(ph.Client);

                        if (FactionPlayerList.TryRemove(playerID.Key, out FactionPlayer value))
                        {
                            client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                            await PriseServiceMenu(client);
                            await UpdateDatabase();
                        }
                        return;
                    }
                    else if ((await Identite.GetOfflineIdentite(playerID.Key)).Name == menuItem.Text)
                    {
                        if (FactionPlayerList.TryRemove(playerID.Key, out FactionPlayer value))
                        {
                            client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                            await PriseServiceMenu(client);
                            await UpdateDatabase();
                        }
                        return;
                    }
                }
            } else if(menuItem.Id == "ID_demissioner")
            {
                foreach (var playerID in FactionPlayerList)
                {
                    if (playerID.Key == menuItem.GetData("playerId"))
                    {
                        if (ServicePlayerList.Contains(playerID.Key))
                        {
                            PlayerHandler ph = PlayerManager.GetPlayerBySCN(playerID.Key);
                            await PriseService(ph.Client);
                        }

                        if (FactionPlayerList.TryRemove(playerID.Key, out FactionPlayer value))
                        {
                            client.DisplayHelp($"Vous avez démissionné de la faction {FactionName}", 10000);
                            await menu.CloseMenu(client);
                            await UpdateDatabase();
                        }
                        return;
                    }
                }
            }
        }

        private async Task OpenVestiaire(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            await menu.CloseMenu(client);

            var _player = client.GetPlayerHandler();

            if (_player == null)
                return;

            var invmenu = new RPGInventoryMenu(_player.PocketInventory, _player.OutfitInventory, _player.BagInventory, FactionPlayerList[client.GetSocialClub()].Inventory);
            invmenu.OnMove += async (p, m) =>
            {
                _player.UpdateFull();
                await UpdateDatabase();
            };
            invmenu.PriceChange += async (p, m, stack, stackprice) =>
            {
                client.SendNotification($"Le nouveau prix de {stack.Item.name} est de ${stackprice} ");
                _player.UpdateFull();
                await UpdateDatabase();
            };
            await invmenu.OpenMenu(client);
        }

        // Depot d'argent dans la caisse.
        private async Task DepotMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (double.TryParse(menuItem.InputValue, out double result))
            {
                var ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                if (result < 0) return;
                if (ph.HasMoney(result))
                {
                    await BankAccount.AddMoney(result, $"Ajout d'argents par {ph.Identite.Name}");
                    ph.UpdateFull();
                    client.SendNotificationSuccess($"Vous avez déposé ${result} dans la caisse.");
                }
                else
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
            }

            await PriseServiceMenu(client);
        }

        // Récupérer l'argent dans la caisse.
        private async Task FinanceMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            await Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Faction, menu, ServiceMenuCallBack);
        }

        public virtual async Task<Menu> OpenShopMenu(IPlayer client)
        {
            if (HasPlayerIntoFaction(client))
            {
                Menu menu = new Menu("ID_Shop", FactionName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
                menu.ItemSelectCallback = ShopMenuCallBack;
                foreach (var item in ItemShop)
                {
                    if (item.Rang > GetRangPlayer(client))
                        continue;

                    MenuItem menuitem = new MenuItem(item.Item.name, item.Item.description, "ID_BuyItem", true, rightLabel: (item.Price > 0) ? $"${item.Price.ToString()}" : "");
                    menuitem.SetData("Item", item);
                    menu.Add(menuitem);
                }

                await menu.OpenMenu(client);
                return menu;
            }
            return null;
        }

        public virtual async Task<Menu> OpenConcessMenu(IPlayer client, ConcessType type, Location location, string factionName)
        {
            if (!HasPlayerIntoFaction(client))
            {
                client.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking!");
                return null;
            }

            if (GetVehicleAllowed(GetRangPlayer(client)) == null)
            {
                client.SendNotificationError("Aucun véhicule d'autorisé");
                return null;
            }

            _factionName = factionName;
            string subtitle;

            if (type == ConcessType.Helico)
                subtitle = $"Parking hélicoptères {factionName}";
            else
                subtitle = $"Parking véhicules {factionName}";

            Menu menu = new Menu("ID_ParkingMenu", "Parking", subtitle, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.SetData("ConcessType", type);
            menu.SetData("Faction_Location", location); 
            menu.ItemSelectCallback = ConcessCallBack;

            if (await client.IsInVehicleAsync())
                menu.Add(new MenuItem("Ranger le véhicule", "", "ID_StoreVehicle", true));
            else
            {
                menu.Add(new MenuItem("Acheter un véhicule", "", "ID_ShopVehicleMenu", true));
                menu.Add(new MenuItem("Sortir un véhicule", "", "ID_OutVehicleMenu", true));
            }

            await menu.OpenMenu(client);

            return menu;
        }
        #endregion

        #region MenuCallBack
        private async Task Dismiss(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex)
        {
            IPlayer target = menu.GetData("Target");
            if (target != null)
            {
                this.FactionPlayerList.Remove( target.GetSocialClub(), out FactionPlayer value);
                target.SendNotification($"Vous avez été congédié de {FactionName}.");
                await UpdateDatabase();
            }
        }

        private async Task ServiceMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await PriseServiceMenu(client);
                return;
            }

            await PriseService(client);
        }

        private async Task ShopMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            FactionShop item = menuItem.GetData("Item");
            PlayerHandler ph =  client.GetPlayerHandler();

            if (item == null || ph == null)
                return;

            if (await BankAccount.GetBankMoney(item.Price, $"Achat de {item.Item.name} par {ph.Identite.Name}"))
            {
                try
                {
                    if (item.Item.type == "weapons")
                        item.Item.isStackable = false;

                    if (await ph.AddItem(item.Item, 1))
                    {
                        client.SendNotificationSuccess($"Vous avez pris un(e) {item.Item.name}");
                    }
                    else
                    {
                        client.SendNotificationError($"Vous n'avez pas la place pour un(e) {item.Item.name}");
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("ShopMenuCallBack " + ex);
                    client.SendNotificationError($"Une erreur s'est produite avec l'item: {item.Item.name}");
                }
            }
            else
                client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
        }

        private async Task ConcessCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_ParkingMenu")
            {
                ConcessType type = (ConcessType)menu.GetData("ConcessType");

                if (menuItem == null)
                {
                    await OpenConcessMenu(client, type, menu.GetData("Faction_Location"), _factionName);
                    return;
                }

                switch (menuItem.Id)
                {
                    case "ID_StoreVehicle":
                        if (client.IsInVehicle)
                        {
                            if (Parking != null)
                                await Parking.StoreVehicle(client, client.Vehicle);

                            await menu.CloseMenu(client);
                        }
                        break;

                    case "ID_OutVehicleMenu":
                        Location loc = menu.GetData("Faction_Location");

                        if (type == ConcessType.Vehicle)
                            Parking?.OpenParkingMenu(client, location: loc, menu: menu, menuCallback: ConcessCallBack);
                        else
                            Parking?.OpenParkingMenu(client, location: loc, vehicleType: 15, menu: menu, menuCallback: ConcessCallBack);

                        break;

                    case "ID_ShopVehicleMenu":
                        var location = menu.GetData("Faction_Location");
                        menu.ClearItems();
                        menu.BackCloseMenu = false;
                        menu.Id = "ID_ConcessMenuChoise";
                        menu.Title = "Concessionnaire";
                        menu.SubTitle = "Quel véhicule souhaitez-vous acheter :";
                        menu.ItemSelectCallback = ConcessCallBack;

                        foreach (FactionVehicle veh in VehicleAllowed)
                        {
                            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)veh.Hash);

                            if (manifest == null)
                                continue;

                            if (type == ConcessType.Helico)
                            {
                                if (manifest.VehicleClass == 15)
                                {
                                    MenuItem item = new MenuItem(string.IsNullOrEmpty(manifest.LocalizedName) ? manifest.DisplayName : manifest.LocalizedName, "", "ID_Buy", true, rightLabel: $"${veh.Price.ToString()}");
                                    item.SetData("Veh", veh);
                                    item.SetData("Manifest", manifest.DisplayName);
                                    menu.Add(item);
                                }
                            }
                            else if (type == ConcessType.Vehicle)
                            {
                                if (manifest.VehicleClass != 15)
                                {
                                    MenuItem item = new MenuItem(string.IsNullOrEmpty(manifest.LocalizedName) ? manifest.DisplayName : manifest.LocalizedName, "", "ID_Buy", true, rightLabel: $"${veh.Price.ToString()}");
                                    item.SetData("Veh", veh);
                                    item.SetData("Manifest", manifest.DisplayName);
                                    menu.Add(item);
                                }
                            }
                        }

                        await menu.OpenMenu(client);
                        break;
                }
            }
            else if (menu.Id == "ID_ConcessMenuChoise")
            {
                Location location = menu.GetData("Faction_Location");

                if (menuItem == null)
                {
                    ConcessType type = (ConcessType)menu.GetData("ConcessType");
                    await OpenConcessMenu(client, type, location, _factionName);
                    return;
                }

                FactionVehicle fv = (FactionVehicle)menuItem.GetData("Veh");
                PlayerHandler ph = client.GetPlayerHandler();
                string vhname = (string)menuItem.GetData("Manifest");

                if (await BankAccount.GetBankMoney(fv.Price, $"Achat véhicule {vhname} par {ph.Identite.Name}"))
                {
                    VehicleHandler vh = await VehiclesManager.SpawnVehicle(client.GetSocialClub(), (uint)fv.Hash, location.Pos, location.Rot, inventory: new Inventory.Inventory(fv.Weight, fv.MaxSlot), primaryColor: fv.PrimaryColor, secondaryColor: fv.SecondaryColor);
                    await vh.InsertVehicle();
                    client.SetPlayerIntoVehicle(vh.Vehicle);
                    await OnVehicleOut(client, vh);
                    ph.ListVehicleKey.Add(new VehicleKey(vhname, vh.Plate));
                    ph.UpdateFull();
                    await MenuManager.CloseMenu(client);
                }
                else
                    client.SendNotificationError("Votre faction n'a pas assez d'argent.");
            }
        }
        #endregion
    }
}
