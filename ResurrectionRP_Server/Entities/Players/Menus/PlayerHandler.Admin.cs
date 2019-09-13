using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Entities.Players
{
    partial class PlayerHandler
    {
        #region Variables
        private PlayerHandler _playerSelected;
        #endregion

        #region MainMenu
        public async Task OpenAdminMenu(PlayerHandler playerSelected = null)
        {
            #region Verification
            if (StaffRank <= AdminRank.Player)
                return;

            if (Client == null)
                return; // idk for what, but maybe.
            
            // if (_playerSelected != null && playerSelected != _playerSelected && _playerSelected.Client != null && _playerSelected.Client.Exists)
            //     _playerSelected.Client.ResetSharedData("AC_Immunity");

            if (_playerSelected == null && playerSelected == null)
                _playerSelected = this;
            else if (playerSelected != null)
                _playerSelected = playerSelected;

            // if (_playerSelected.Client != null && _playerSelected.Client.Exists && !_playerSelected.Client.HasSharedData("AC_Immunity"))
            //     _playerSelected.Client.SetSharedData("AC_Immunity", true);
            
            #endregion

            #region Menu
            Menu mainMenu = new Menu("ID_AdminMenu", "Admin Menu", "Choisissez une option:", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.Garage);
            mainMenu.SubTitle = $"Joueur Selectionné: ~r~{_playerSelected.Identite.Name}";
            mainMenu.Finalizer += OnFinalize;
            #endregion

            #region Choix du joueur
            var pchoise = new MenuItem("Choix du Joueur", "", "", true);
            pchoise.OnMenuItemCallback = ChoisePlayer;
            mainMenu.Add(pchoise);
            #endregion

            #region StaffRank
            if (StaffRank >= AdminRank.Developer)
            {
                List<object> _adminlistrang = new List<object>();
                foreach (int value in Enum.GetValues(typeof(AdminRank)))
                {
                    if ((int)StaffRank > value)
                        _adminlistrang.Add(((AdminRank)value).ToString());
                }

                var staffrank = new ListItem("Staff Rang:", "Choix du rang à donner", "ID_Rang", _adminlistrang, (int)StaffRank, true);
                staffrank.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    AdminRank rang = (AdminRank)((ListItem)menu.Items["ID_Rang"]).SelectedItem;
                    _playerSelected.StaffRank = rang;
                    _playerSelected.Client.SendNotification($"Vous êtes désormais {rang}");
                    client.SendNotification($"Vous avez mis au rang: {rang} {_playerSelected.Identite.Name}");
                    await _playerSelected.Update();
                };

                mainMenu.Add(staffrank);
            }
            #endregion

            #region Message Global
            var globalAnnonce = new MenuItem("Message Global", "", "ID_Global", true);
            globalAnnonce.SetInput("Votre message", 99, InputType.Text);
            globalAnnonce.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                if (string.IsNullOrEmpty(menuItem.InputValue)) return;

                foreach (var player in Alt.GetAllPlayers())
                {
                    if (!player.Exists)
                        continue;
                    await player.EmitAsync(Events.AnnonceGlobal, menuItem.InputValue, "AVIS A LA POPULATION!", "COMMUNIQUÉ GOUVERNEMENTAL");
                }
            };
            mainMenu.Add(globalAnnonce);
            #endregion

            #region Reset Life
            var lifeitem = new MenuItem("Reset life", "", "", true);
            lifeitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                 _playerSelected.Health = 100;
                 await Update();
            };
            mainMenu.Add(lifeitem);
            #endregion

            #region Reset Thirst & Hunger
            var hungeritem = new MenuItem("Reset faim et soif", "", "", true);
            hungeritem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                await _playerSelected.UpdateHungerThirst(100, 100);
            };
            mainMenu.Add(hungeritem);
            #endregion
            /*
            #region GodMod
            var godMod = new CheckboxItem("God Mode", "", "", _playerSelected.IsInvinsible(), true);
            godMod.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                bool invinsible = !_playerSelected.IsInvinsible();
                await _playerSelected.SetInvincible(invinsible);

                if (invinsible)
                {
                    await _playerSelected.Client.NotifyAsync("~r~[ADMIN]~w~ Vous êtes invincible.");
                    // LogManager.Log($"~r~[ADMIN]~w~ {PlayerSelected.Name} est invincible.");

                    if (_playerSelected != this)
                        await Client.NotifyAsync($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} est invincible.");
                }
                else
                {
                    await _playerSelected.Client.NotifyAsync("~r~[ADMIN]~w~ Vous n'êtes plus invincible.");
                    // LogManager.Log($"~r~[ADMIN]~w~ {PlayerSelected.Name} n'est plus invincible.");

                    if (_playerSelected != this)
                        await Client.NotifyAsync($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} n'est plus invincible.");
                }
            };
            mainMenu.Add(godMod);
            #endregion

            #region Invisible
            var invisible = new CheckboxItem("Invisible", "", "", await _playerSelected.Client.IsInvisible(), true);
            invisible.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                bool hidden = ((CheckboxItem)menuItem).Checked;
                await _playerSelected.Client.SetInvisible(hidden);

                if (hidden)
                {
                    await _playerSelected.Client.NotifyAsync("~r~[ADMIN]~w~ Vous êtes invisible.");
                    // LogManager.Log($"~r~[ADMIN]~w~ {PlayerSelected.Name} est invisible par " + client.Name);

                    if (_playerSelected != this)
                        await Client.NotifyAsync($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} est invisible.");
                }
                else
                {
                    await _playerSelected.Client.NotifyAsync("~r~[ADMIN]~w~ Vous n'êtes plus invisible.");
                    // LogManager.Log($"~r~[ADMIN]~w~ {PlayerSelected.Name} n'êtes plus invicible par " + client.Name);

                    if (_playerSelected != this)
                        await Client.NotifyAsync($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} n'est plus invisible.");
                }
            };
            mainMenu.Add(invisible);
            #endregion
            */
            #region VehicleUnlock
            var vehunlock = new MenuItem("(Un)Lock Véhicule", "", "", true);
            vehunlock.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                var veh = await VehiclesManager.GetNearestVehicle(client);
                if (veh != null)
                {    
                    await veh.SetLockStateAsync(veh.LockState == VehicleLockState.Locked ? VehicleLockState.Unlocked : VehicleLockState.Locked);
                    client.SendNotificationSuccess($"Vous venez {(veh.LockState == VehicleLockState.Locked ? "de fermer" : "d'ouvrir")} le véhicule {veh.NumberplateText}");
                    //LogManager.Log($"~r~[ADMIN]~w~ {client.Name} viens d'ouvrir ou fermer " + VehiclesManager.GetNearestVehicle(client).NumberPlate);
                }
                else
                    client.SendNotificationError("Aucun véhicule a votre portée.");
            };
            mainMenu.Add(vehunlock);
            #endregion

            #region Waypoint
            var waypoint = new MenuItem("Téléporter sur le Waypoint", "", "", true);
            waypoint.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                await _playerSelected.Client.EmitAsync(Events.SetToWayPoint);
            };
            mainMenu.Add(waypoint);
            #endregion

            #region TeleportToMe
            var tptome = new MenuItem("Téléporter le joueur à moi", "", "", true);
            tptome.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                Vector3 playerpos = await Client.GetPositionAsync();
                playerpos.X = playerpos.X + 1f;
                await _playerSelected.Client.SetPositionAsync(playerpos);
                await _playerSelected.Client.SetDimensionAsync(await Client.GetDimensionAsync());
                Client.SendNotificationSuccess($"{_playerSelected.Identite.Name} viens d'être téléporté sur vous");
            };
            mainMenu.Add(tptome);
            #endregion

            #region TeleportMe
            var tpme = new MenuItem("Téléporter sur le joueur", "", "", true);
            tpme.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                await Client.SetPositionAsync(await _playerSelected.Client.GetPositionAsync());
                await Client.SetDimensionAsync(await _playerSelected.Client.GetDimensionAsync());
                Client.SendNotificationSuccess($"vous venez d'être téléporté sur {_playerSelected.Identite.Name}");
            };
            mainMenu.Add(tpme);
            #endregion

            #region Weapon
            var weapon = new MenuItem("Prendre une arme", "", "ID_Weapon", true);
            weapon.SetInput("", 30, InputType.Text);
            weapon.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                string name = menuItem.InputValue;

                if (string.IsNullOrEmpty(name))
                    return;

                if (Enum.TryParse(name, true, out WeaponHash hash) == false)
                {
                    client.SendNotificationError($"Weapon {menuItem.InputValue} is invalid!");
                    return;
                }

                await _playerSelected.Client.GiveWeaponAsync((uint)hash, 200, true);
            };
            mainMenu.Add(weapon);
            #endregion

            #region Ped
            var peditem = new MenuItem("Prendre l'apparence d'un ped", "", "ID_Ped", true);
            peditem.SetInput("", 30, InputType.Text);
            peditem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                string name = menuItem.InputValue;

                if (string.IsNullOrEmpty(name))
                    return;

                if (Enum.TryParse(name, true, out PedModel hash) == false)
                {
                    client.SendNotificationError($"Ped {menuItem.InputValue} is invalid!");
                    return;
                }

                await _playerSelected.Client.SetModelAsync((uint)hash);
            };
            mainMenu.Add(peditem);
            #endregion

            #region Kill
            var killitem = new MenuItem("Kill", "", "", true);
            killitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                client.SendNotificationSuccess($"Vous venez de tuer {_playerSelected.Identite.Name}.");
                await _playerSelected.Client.SetHealthAsync(0);
            };
            mainMenu.Add(killitem);
            #endregion

            #region Kick
            var kickitem = new MenuItem("Kick le joueur", "", "", true);
            kickitem.SetInput("", 99, InputType.Text);
            kickitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                client.SendNotificationSuccess($"Vous venez de kick {_playerSelected.Identite.Name}.");
                await _playerSelected.Client.KickAsync(menuItem.InputValue);
            };
            mainMenu.Add(kickitem);
            #endregion

            #region Ban
            var banitem = new MenuItem("Ban le joueur", "", "", true);
            banitem.SetInput("", 99, InputType.Text);
            banitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                if(string.IsNullOrEmpty(menuItem.InputValue)) return;
                client.SendNotificationSuccess($"Vous venez de ban {_playerSelected.Identite.Name}.");
                await BanManager.BanPlayer(_playerSelected.Client, menuItem.InputValue, new DateTime(2031, 1, 1));
                // TODO ajouter contacte API
            };
            mainMenu.Add(banitem);
            #endregion

            #region Money
            var moneyitem = new MenuItem("Give de l'argent", "", "ID_GiveMoney", true);
            moneyitem.SetInput("", 10, InputType.UFloat, true);
            moneyitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) => 
            {
                double money = 0;

                if (double.TryParse(menuItem.InputValue, out money) && money > 0)
                {
                    await _playerSelected.AddMoney(money);
                    client.SendNotificationSuccess($"Vous venez de donner {money} à {_playerSelected.Identite.Name}");
                }
            };
            mainMenu.Add(moneyitem);
            #endregion

            #region Remove Money
            var delmoneyitem = new MenuItem("Retirer de l'argent", "", "ID_RemoveMoney", true);
            delmoneyitem.SetInput("", 10, InputType.UFloat, true);
            delmoneyitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                double money = 0;

                if (double.TryParse(menuItem.InputValue, out money) && money > 0)
                {
                    if (await _playerSelected.HasMoney(money))
                        client.SendNotificationSuccess($"Vous venez de retirer ${money} à {_playerSelected.Identite.Name}");
                }
            };
            mainMenu.Add(delmoneyitem);
            #endregion

            #region NoClip
            var noclipitem = new MenuItem("NoClip", "", "", true);
            noclipitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                await _playerSelected.Client.EmitAsync("ToggleNoclip");
            };
            mainMenu.Add(noclipitem);
            #endregion

            #region Revive
            var resuitem = new MenuItem("Réanimmer le joueur", "", "", true);
            resuitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                await _playerSelected.Client.Revive();
            };
            mainMenu.Add(resuitem);
            #endregion

            #region Spawn Provisoire
            var spawn = new MenuItem("Spawn voiture temporaire", "Spawn une voiture avec le nom rentré, jusqu'au reboot.", "ID_SpawnVeh", true);
            spawn.SetInput("", 30, InputType.Text);
            spawn.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                try
                {
                    string name = menuItem.InputValue;
                    if (string.IsNullOrEmpty(name)) return;
                    uint hash = Alt.Hash(name);
                    VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)hash);

                    if (manifest == null)
                    {
                        client.SendNotificationError($"véhicule inconnu : {name}");
                        return;
                    }

                    VehicleHandler vehicle = await VehiclesManager.SpawnVehicle(_playerSelected.Client.GetSocialClub(), hash, await _playerSelected.Client.GetPositionAsync(), await _playerSelected.Client.GetRotationAsync(), fuel: 100, fuelMax: 100, spawnVeh: true);

                    if (vehicle != null)
                    {
                        _playerSelected.Client.SetPlayerIntoVehicle(vehicle.Vehicle);
                        _playerSelected.ListVehicleKey.Add(new VehicleKey(manifest.DisplayName, vehicle.Plate));
                        //LogManager.Log($"~r~[ADMIN]~w~ {client.Name} a spawn le véhicule {_vehs.Model} {_vehs.Plate}");
                    }
                    else
                        client.SendNotificationError("Il y a une erreur avec le véhicule demandé.");
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"ADMIN SPAWN VEHICLE PROVISOIRE: {ex}");
                }
            };
            mainMenu.Add(spawn);
            #endregion

            #region Spawn Perm
            var spawnPerm = new MenuItem("Spawn voiture", "Spawn une voiture avec le nom rentré.", "ID_SpawnVehPerm", true);
            spawnPerm.SetInput("", 30, InputType.Text);
            spawnPerm.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                try
                {
                    string name = menuItem.InputValue;

                    if (string.IsNullOrEmpty(name))
                        return;

                    uint hash = Alt.Hash(name);
                    VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(hash);

                    if (manifest == null)
                    {
                        client.SendNotificationError($"véhicule inconnu : {name}");
                        return;
                    }

                    VehicleHandler vehicle = await VehiclesManager.SpawnVehicle(_playerSelected.Client.GetSocialClub(), hash, await _playerSelected.Client.GetPositionAsync(), await _playerSelected.Client.GetRotationAsync(), fuel: 100, fuelMax: 100, spawnVeh: false);

                    if (vehicle != null)
                    {
                        _playerSelected.Client.SetPlayerIntoVehicle(vehicle.Vehicle);
                        await vehicle.InsertVehicle();
                        _playerSelected.ListVehicleKey.Add(new VehicleKey(manifest.DisplayName, vehicle.Plate));
                        //LogManager.Log($"~r~[ADMIN]~w~ {client.Name} a spawn le véhicule {_vehs.Model} {_vehs.Plate}");
                    }
                    else
                    {
                        client.SendNotificationError("Il y a une erreur avec le véhicule demandé.");
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"ADMIN SPAWN VEHICLE PERSISTANT: {ex}");
                }
            };
            mainMenu.Add(spawnPerm);
            #endregion

            #region Delete Vehicle Perm
            var deletepermvehitem = new MenuItem("Supprimer le véhicule (PERM).", "~r~ATTENTION CECI LE RETIRE DE LA BASE DE DONNÉES!", "", true);
            deletepermvehitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                VehicleHandler vehicle = (await VehiclesManager.GetNearestVehicle(client)).GetVehicleHandler();

                if (vehicle == null)
                    client.SendNotificationError("Aucun véhicule a proximité");
                else if (await vehicle.Delete(true))
                    client.SendNotificationSuccess($"Véhicule ~r~{vehicle.Plate}~w~ supprimé...");
                else
                    client.SendNotificationError($"Erreur de suppression du véhicule");
            };

            mainMenu.Add(deletepermvehitem);
            #endregion

            #region Delete Vehicle
            var deletevehitem = new MenuItem("Supprimer le véhicule.", "", "", true);
            deletevehitem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                VehicleHandler vehicle = (await VehiclesManager.GetNearestVehicle(client)).GetVehicleHandler();

                if (vehicle == null)
                    client.SendNotificationError("Aucun véhicule a proximité");
                else if (await vehicle.Delete(false))
                    client.SendNotificationSuccess($"Véhicule ~r~{vehicle.Plate}~w~ supprimé...");
                else
                    client.SendNotificationError($"Erreur de suppression du véhicule");
            };

            mainMenu.Add(deletevehitem);
            #endregion

            #region Pound
            var pounditem = new MenuItem("Mettre en fourrière le véhicule", "", "", true);
            pounditem.OnMenuItemCallback = async (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
            {
                IVehicle vehicle = await VehiclesManager.GetNearestVehicle(client);

                if (vehicle == null)
                {
                    client.SendNotificationError("Aucun véhicule a proximité");
                    return;
                }
                else
                {
                    VehicleHandler vehfourriere = (await VehiclesManager.GetNearestVehicle(client)).GetVehicleHandler();

                    if (vehfourriere.SpawnVeh)
                        return;

                    client.SendNotification($"Véhicule ~r~{vehfourriere.Plate} ~w~ mis en fourrière...");
                    // await GameMode.Instance.PoundManager.AddVehicleInPound(vehfourriere);
                }
            };

            mainMenu.Add(pounditem);
            #endregion

            #region Fin
            await mainMenu.OpenMenu(Client);
            #endregion
        }

        private Task OnFinalize(IPlayer client, Menu menu)
        {
            /*
            if (_playerSelected.Client != null && _playerSelected.Client.Exists)
                _playerSelected.Client.ResetSharedData("AC_Immunity");
            */
            return Task.CompletedTask;
        }
        #endregion

        #region CallBacks
        private async Task ChoisePlayer(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var secondMenu = new Menu("", "Admin Menu", "Choisissez un joueur :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Garage);
            secondMenu.ItemSelectCallback = ChoisePlayerCallback;

            List<PlayerHandler> players = PlayerManager.GetPlayersList();

            foreach (PlayerHandler player in players)
            {
                if (player != null && player.Client.Exists)
                {
                    string description = "";
                    var item = new MenuItem(player.Identite.Name, description, executeCallback: true);
                    item.OnMenuItemCallback = async (IPlayer cClient, Menu cMenu, IMenuItem cMenuItem, int _itemIndex) =>
                    {
                        PlayerHandler playerSelected = players[_itemIndex];

                        if (!playerSelected.Client.Exists)
                            return;

                        await OpenAdminMenu(playerSelected);
                    };

                    secondMenu.Add(item);
                }
            }

            await secondMenu.OpenMenu(Client);
        }

        private async Task ChoisePlayerCallback(IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex)
        {
            if (menuItem == null)
                await OpenAdminMenu();
        }
        #endregion
    }
}
