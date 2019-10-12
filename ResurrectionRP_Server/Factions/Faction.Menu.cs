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

        private void AddFactionTargetMenu_Callback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            try
            {
                IPlayer target = menu.GetData("Faction_Target");
                if (target == null) return;

                menu = new XMenu("ID_Faction");
                InteractPlayerMenu(client, target, menu);
                menu.OpenXMenu(client);
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

        private void RankChangeChoise(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            if (_target == null) return;

            var clientRank =  GetRangPlayer(client);

            menu = new XMenu("");
            menu.SetData("Player", _target);

            var dismiss = new XMenuItem("Renvoyer", executeCallback: true);
            dismiss.OnMenuItemCallbackAsync = DissMissPlayer;
            menu.Add(dismiss);

            foreach (var rang in FactionRang)
            {
                if (rang.Rang > clientRank) continue;

                var item = new XMenuItem(rang.RangName, "", "", XMenuItemIcons.CHECK);
                item.OnMenuItemCallbackAsync = RankChange;
                item.SetData("Rang", rang);
                menu.Add(item);
            }

            menu.OpenXMenu(client);
        }

        private Task DissMissPlayer(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");

            if (_target == null)
                return Task.CompletedTask;

            FactionPlayerList.Remove( _target.GetSocialClub(), out FactionPlayer value);
            _target.SendNotification($"Vous avez été renvoyé de {FactionName}.");
            UpdateInBackground();
            return Task.CompletedTask;
        }

        private void InviteFactionChoise(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
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

            menu.OpenXMenu(client);
        }

        private void InviteFaction(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");

            if (_target == null)
                return;

            FactionRang rang = menuItem.GetData("Rang");

            AcceptMenu accept = AcceptMenu.OpenMenu(_target, $"{FactionName}", $"Rejoindre la faction {FactionName} rang {rang.RangName}?");
            accept.AcceptMenuCallBack = async (IPlayer c, bool reponse) =>
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
            };
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
            UpdateInBackground();
        }

        private Task Dismiss(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex)
        {
            IPlayer target = menu.GetData("Target");

            if (target != null)
            {
                FactionPlayerList.Remove(target.GetSocialClub(), out FactionPlayer value);
                target.SendNotification($"Vous avez été congédié de {FactionName}.");
                UpdateInBackground();
            }

            return Task.CompletedTask;
        }
        #endregion

        #region VehicleMenu
        public XMenu AddFactionVehicleMenu(IPlayer client, IVehicle vehicle, XMenu xMenu, XMenuItemIconDesc icon)
        {
            if ( HasPlayerIntoFaction(client))
            {
                var item = new XMenuItem(FactionName, "", "", icon, true);
                item.OnMenuItemCallbackAsync = AddFactionVehicleMenu_Callback;
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
                menu.OpenXMenu(client);
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

        #region Service
        public virtual Menu PriseServiceMenu(IPlayer client)
        {
            if (HasPlayerIntoFaction(client))
            {
                Menu menu = new Menu("ID_ServiceMenu", FactionName, "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);

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
                    gestion.OnMenuItemCallbackAsync = GestionMember;
                    menu.Add(gestion);
                }

                var vestiaire = new MenuItem("Ouvrir votre vestiaire", "", "ID_Vestiaire", true);
                vestiaire.OnMenuItemCallback = OpenVestiaire;
                menu.Add(vestiaire);

                var demission = new MenuItem("Démissioner", "", "ID_demissioner", executeCallback: true);
                demission.OnMenuItemCallbackAsync = GestionMemberCallback;
                demission.SetData("playerId", client.GetSocialClub());
                menu.Add(demission);
                menu.OpenMenu(client);

                return menu;
            }
            else
                client.SendNotificationError("Vous ne faites pas partie de cette faction.");

            return null;
        }

        private void ServiceMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                PriseServiceMenu(client);
                return;
            }

            Task.Run(async () => { await PriseService(client); });
        }

        private void OpenVestiaire(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            menu.CloseMenu(client);

            var _player = client.GetPlayerHandler();

            if (_player == null)
                return;

            var invmenu = new RPGInventoryMenu(_player.PocketInventory, _player.OutfitInventory, _player.BagInventory, FactionPlayerList[client.GetSocialClub()].Inventory);
            invmenu.OnMove += (p, m) =>
            {
                _player.UpdateFull();
                UpdateInBackground();
            };
            invmenu.PriceChange += (p, m, stack, stackprice) =>
            {
                client.SendNotification($"Le nouveau prix de {stack.Item.name} est de ${stackprice} ");
                _player.UpdateFull();
                UpdateInBackground();
            };
            invmenu.OpenMenu(client);
        }

        // Depot d'argent dans la caisse.
        private void DepotMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (double.TryParse(menuItem.InputValue, out double result))
            {
                var ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                if (result < 0)
                    return;

                if (ph.HasMoney(result))
                {
                    BankAccount.AddMoney(result, $"Ajout d'argents par {ph.Identite.Name}");
                    ph.UpdateFull();
                    client.SendNotificationSuccess($"Vous avez déposé ${result} dans la caisse.");
                }
                else
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
            }

            PriseServiceMenu(client);
        }

        // Récupérer l'argent dans la caisse.
        private void FinanceMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Faction, menu, ServiceMenuCallBack);
        }
        #endregion

        #region Members
        private async Task GestionMember(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.SubTitle = "Gestion des membres";
            menu.ItemSelectCallbackAsync = GestionMemberCallback;

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

            menu.OpenMenu(client);
        }

        private async Task GestionMemberCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                PriseServiceMenu(client);
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
                            UpdateInBackground();
                        }
                    }
                    else
                        client.SendNotificationError($"{_msg} est introuvable.");
                }
                else
                    client.SendNotificationError("Aucun nom de rentré.");

                PriseServiceMenu(client);
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
                            PriseServiceMenu(client);
                            UpdateInBackground();
                        }

                        return;
                    }
                    else if ((await Identite.GetOfflineIdentite(playerID.Key)).Name == menuItem.Text)
                    {
                        if (FactionPlayerList.TryRemove(playerID.Key, out FactionPlayer value))
                        {
                            client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                            PriseServiceMenu(client);
                            UpdateInBackground();
                        }

                        return;
                    }
                }
            }
            else if(menuItem.Id == "ID_demissioner")
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
                            menu.CloseMenu(client);
                            UpdateInBackground();
                        }

                        return;
                    }
                }
            }
        }
        #endregion

        #region Shop
        public virtual void OpenShopMenu(IPlayer client)
        {
            if (!HasPlayerIntoFaction(client))
                return;

            Menu menu = new Menu("ID_Shop", FactionName, "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.ItemSelectCallback = ShopMenuCallBack;

            foreach (FactionShopItem item in ItemShop)
            {
                if (item.Rang > GetRangPlayer(client))
                    continue;

                MenuItem menuitem = new MenuItem(item.Item.name, item.Item.description, "ID_BuyItem", true, rightLabel: (item.Price > 0) ? $"${item.Price.ToString()}" : "");
                menuitem.SetData("Item", item);
                menu.Add(menuitem);
            }

            menu.OpenMenu(client);
        }

        private void ShopMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            FactionShopItem item = menuItem.GetData("Item");
            PlayerHandler ph = client.GetPlayerHandler();

            if (item == null || ph == null)
                return;

            if (BankAccount.GetBankMoney(item.Price, $"Achat de {item.Item.name} par {ph.Identite.Name}"))
            {
                try
                {
                    if (item.Item.type == "weapons")
                        item.Item.isStackable = false;

                    if (ph.AddItem(item.Item, 1))
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
        #endregion

        #region Parking
        public virtual void OpenConcessMenu(IPlayer client, ConcessType type, Location location, string factionName)
        {
            if (client == null || !client.Exists)
                return;
            else if (!HasPlayerIntoFaction(client))
            {
                client.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking!");
                return;
            }
            else if (GetVehicleAllowed(GetRangPlayer(client)) == null)
            {
                client.SendNotificationError("Aucun véhicule d'autorisé");
                return;
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
            menu.ItemSelectCallbackAsync = ConcessCallBack;

            if (client.IsInVehicle)
                menu.Add(new MenuItem("Ranger le véhicule", "", "ID_StoreVehicle", true));
            else
            {
                menu.Add(new MenuItem("Acheter un véhicule", "", "ID_ShopVehicleMenu", true));
                menu.Add(new MenuItem("Sortir un véhicule", "", "ID_OutVehicleMenu", true));
            }

            menu.OpenMenu(client);
        }

        private async Task ConcessCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenConcessMenu(client, (ConcessType)menu.GetData("ConcessType"), menu.GetData("Faction_Location"), _factionName);
                return;
            }

            if (menu.Id == "ID_ParkingMenu")
            {
                ConcessType type = (ConcessType)menu.GetData("ConcessType");

                if (menuItem == null)
                {
                    OpenConcessMenu(client, type, menu.GetData("Faction_Location"), _factionName);
                    return;
                }

                switch (menuItem.Id)
                {
                    case "ID_StoreVehicle":
                        if (client.IsInVehicle)
                        {
                            if (Parking != null)
                            {
                                if (type == ConcessType.Helico)
                                    await Parking.StoreVehicle(client, client.Vehicle, HeliportLocation);
                                else
                                    await Parking.StoreVehicle(client, client.Vehicle);
                            }

                            menu.CloseMenu(client);
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
                        menu.ItemSelectCallbackAsync = ConcessCallBack;

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

                        menu.OpenMenu(client);
                        break;
                }
            }
            else if (menu.Id == "ID_ConcessMenuChoise")
            {
                Location location = menu.GetData("Faction_Location");

                if (menuItem == null)
                {
                    ConcessType type = (ConcessType)menu.GetData("ConcessType");
                    OpenConcessMenu(client, type, location, _factionName);
                    return;
                }

                FactionVehicle fv = (FactionVehicle)menuItem.GetData("Veh");
                PlayerHandler ph = client.GetPlayerHandler();
                string vhname = (string)menuItem.GetData("Manifest");

                if (BankAccount.GetBankMoney(fv.Price, $"Achat véhicule {vhname} par {ph.Identite.Name}"))
                {
                    VehicleHandler vh = VehiclesManager.SpawnVehicle(client.GetSocialClub(), (uint)fv.Hash, location.Pos, location.Rot, inventory: new Inventory.Inventory(fv.Weight, fv.MaxSlot), primaryColor: fv.PrimaryColor, secondaryColor: fv.SecondaryColor);
                    await vh.InsertVehicle();
                    client.SetPlayerIntoVehicle(vh.Vehicle);
                    await OnVehicleOut(client, vh);
                    ph.ListVehicleKey.Add(new VehicleKey(vhname, vh.Plate));
                    ph.UpdateFull();
                    MenuManager.CloseMenu(client);
                }
                else
                    client.SendNotificationError("Votre faction n'a pas assez d'argent.");
            }
        }
        #endregion
    }
}
