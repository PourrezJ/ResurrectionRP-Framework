using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Services;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Phone;

namespace ResurrectionRP_Server.Entities.Players
{
    partial class PlayerHandler
    {
        #region Variables
        private PlayerHandler _playerSelected;
        #endregion

        #region MainMenu
        public void OpenAdminMenu(PlayerHandler playerSelected = null)
        {
            #region Verification
            if (StaffRank < StaffRank.Animator)
                return;

            if (_playerSelected == null && playerSelected == null)
                _playerSelected = this;
            else if (playerSelected != null)
                _playerSelected = playerSelected;

            bool isInvincible = _playerSelected.Client.IsInvinsible();
            bool isInvisible = _playerSelected.Client.IsInvisible();
            #endregion

            #region Menu
            Menu mainMenu = new Menu("ID_AdminMenu", "Admin Menu", "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.Garage)
            {
                SubTitle = $"Joueur Selectionné: ~r~{_playerSelected.Identite.Name}"
            };
            mainMenu.Finalizer += OnFinalize;
            #endregion

            if (StaffRank >= StaffRank.Animator)
            {
                #region Ped
                var peditem = new MenuItem("Prendre l'apparence d'un ped", "", "ID_Ped", true);
                peditem.SetInput("", 30, InputType.Text);
                peditem.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    string name = menuItem.InputValue;

                    if (string.IsNullOrEmpty(name))
                        return;

                    if (Enum.TryParse(name, true, out PedModel hash) == false)
                    {
                        client.SendNotificationError($"Ped {menuItem.InputValue} is invalid!");
                        return;
                    }

                    _playerSelected.Client.Model = ((uint)hash);
                };
                mainMenu.Add(peditem);
                #endregion

                #region Weapon
                var weapon = new MenuItem("Prendre une arme", "", "ID_Weapon", true);
                weapon.SetInput("", 30, InputType.Text);
                weapon.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    string name = menuItem.InputValue;

                    if (string.IsNullOrEmpty(name))
                        return;

                    if (Enum.TryParse(name, true, out WeaponHash hash) == false)
                    {
                        client.SendNotificationError($"Weapon {menuItem.InputValue} is invalid!");
                        return;
                    }

                    _playerSelected.Client.GiveWeapon((uint)hash, 200, true);
                };
                mainMenu.Add(weapon);
                #endregion

                #region Spawn Provisoire
                var spawn = new MenuItem("Spawn véhicule temporaire", "Spawn un véhicule avec le nom rentré, jusqu'au reboot.", "ID_SpawnVeh", true);
                spawn.SetInput("", 30, InputType.Text);
                spawn.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
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

                        Rotation rot = _playerSelected.Client.Rotation;
                        VehicleHandler vehicle = VehiclesManager.SpawnVehicle(_playerSelected.Client.GetSocialClub(), hash, _playerSelected.Client.Position, new Rotation(rot.Pitch, rot.Roll, -rot.Yaw), fuel: 100, fuelMax: 100, spawnVeh: true);

                        if (vehicle != null)
                        {
                            vehicle.LockState = VehicleLockState.Unlocked;
                            _playerSelected.Client.SetPlayerIntoVehicle(vehicle);
                            _playerSelected.ListVehicleKey.Add(new VehicleKey(manifest.DisplayName, vehicle.NumberplateText));
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
            }

            if (StaffRank >= StaffRank.Helper)
            {
                #region Message Global
                var globalAnnonce = new MenuItem("Message Global", "", "ID_Global", true);
                globalAnnonce.SetInput("Votre message", 99, InputType.Text);
                globalAnnonce.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    if (string.IsNullOrEmpty(menuItem.InputValue))
                        return;

                    Alt.EmitAllClients(Events.AnnonceGlobal, menuItem.InputValue, "AVIS A LA POPULATION!", "COMMUNIQUÉ GOUVERNEMENTAL");
                };
                mainMenu.Add(globalAnnonce);
                #endregion

                #region VehicleUnlock
                var vehUnlock = new MenuItem("(Un)Lock Véhicule", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        IVehicle veh = client.GetNearestVehicle(5);

                        if(veh != null) {
                            veh.LockState = veh.LockState == VehicleLockState.Locked ? VehicleLockState.Unlocked : VehicleLockState.Locked;
                            client.SendNotificationSuccess($"Vous venez {(veh.LockState == VehicleLockState.Locked ? "de fermer" : "d'ouvrir")} le véhicule {veh.NumberplateText}");
                        } else
                            client.SendNotificationError("Aucun véhicule a votre portée.");
                    }
                };
                mainMenu.Add(vehUnlock);
                #endregion

                #region Pound
                var pounditem = new MenuItem("Mettre en fourrière le véhicule", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        IVehicle vehicle = client.GetNearestVehicle(5);

                        if(vehicle == null) {
                            client.SendNotificationError("Aucun véhicule a proximité");
                            return;
                        } else {
                            VehicleHandler vehFourriere = vehicle.GetVehicleHandler();

                            if(vehFourriere == null)
                                return;
                            else if(vehFourriere.SpawnVeh) {
                                client.SendNotificationError($"Véhicule admin, mise en fourrière impossible");
                                return;
                            }

                            client.SendNotification($"Véhicule ~r~{vehFourriere.VehicleData.Plate} ~w~ mis en fourrière...");
                            Task.Run(async () => await Pound.AddVehicleInPoundAsync(vehFourriere.VehicleData));
                        }
                    }
                };

                mainMenu.Add(pounditem);
                #endregion
            }

            if (StaffRank >= StaffRank.Moderator)
            {
                #region Choix du joueur
                var pchoise = new MenuItem("Choix du Joueur", "", "", true)
                {
                    OnMenuItemCallback = ChoisePlayer
                };
                mainMenu.Add(pchoise);
                #endregion

                #region StaffRank
                if (StaffRank >= StaffRank.Developer)
                {
                    List<object> _adminlistrang = new List<object>();
                    foreach (int value in Enum.GetValues(typeof(StaffRank)))
                    {
                        if ((int)StaffRank > value)
                            _adminlistrang.Add(((StaffRank)value).ToString());
                    }

                    var staffrank = new ListItem("Staff Rang:", "Choix du rang à donner", "ID_Rang", _adminlistrang, (int) StaffRank, true)
                    {
                        OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                        {
                            StaffRank rang = (StaffRank) ((ListItem) menu.Items["ID_Rang"]).SelectedItem;
                            _playerSelected.StaffRank = rang;
                            _playerSelected.Client.SendNotification($"Vous êtes désormais {rang}");
                            client.SendNotification($"Vous avez mis au rang: {rang} {_playerSelected.Identite.Name}");
                            _playerSelected.UpdateFull();
                        }
                    };

                    mainMenu.Add(staffrank);
                }
                #endregion

                #region Reset Life
                var lifeItem = new MenuItem("Reset life", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        _playerSelected.Health = 200;
                        _playerSelected.Client.Health = 200;
                        UpdateFull();
                    }
                };
                mainMenu.Add(lifeItem);
                #endregion

                #region Reset Thirst & Hunger
                var hungerItem = new MenuItem("Reset faim et soif", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        _playerSelected.UpdateHungerThirst(100, 100);
                    }
                };
                mainMenu.Add(hungerItem);
                #endregion

                #region GodMod
                var godMod = new CheckboxItem("God Mode", "", "", isInvincible, true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        isInvincible = !isInvincible;
                        _playerSelected.Client.SetInvincible(isInvincible);

                        if(isInvincible) {
                            _playerSelected.Client.SendNotification("~r~[ADMIN]~w~ Vous êtes invincible.");

                            if(_playerSelected != this)
                                Client.SendNotification($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} est invincible.");
                        } else {
                            _playerSelected.Client.SendNotification("~r~[ADMIN]~w~ Vous n'êtes plus invincible.");

                            if(_playerSelected != this)
                                Client.SendNotification($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} n'est plus invincible.");
                        }
                    }
                };
                mainMenu.Add(godMod);
                #endregion

                #region Invisible
                var invisible = new CheckboxItem("Invisible", "", "", isInvisible, true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        isInvisible = !isInvisible;
                        _playerSelected.Client.SetInvisible(isInvisible);

                        ; if(isInvisible) {
                            _playerSelected.Client.Dimension = GameMode.GlobalDimension - 1;
                            _playerSelected.Client.SendNotification("~r~[ADMIN]~w~ Vous êtes invisible.");
                            if(_playerSelected != this)
                                Client.SendNotification($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} est invisible.");
                        } else {
                            _playerSelected.Client.Dimension = GameMode.GlobalDimension;
                            _playerSelected.Client.SendNotification("~r~[ADMIN]~w~ Vous n'êtes plus invisible.");
                            if(_playerSelected != this)
                                Client.SendNotification($"~r~[ADMIN]~w~ {_playerSelected.Identite.Name} n'est plus invisible.");
                        }
                    }
                };
                mainMenu.Add(invisible);
                #endregion

                #region Waypoint
                var waypoint = new MenuItem("Téléporter sur le Waypoint", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        if(_playerSelected.Vehicle != null)
                            _playerSelected.Vehicle.WasTeleported = true;

                        _playerSelected.Client.Emit(Events.SetToWayPoint);
                    }
                };
                mainMenu.Add(waypoint);
                #endregion

                #region TeleportToMe
                var tptome = new MenuItem("Téléporter le joueur à moi", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        Vector3 playerpos = client.Position;
                        playerpos.X = playerpos.X + 1f;
                        _playerSelected.Client.Position = (playerpos);
                        _playerSelected.Client.Dimension = Client.Dimension;
                        Client.SendNotificationSuccess($"{_playerSelected.Identite.Name} viens d'être téléporté sur vous");
                    }
                };
                mainMenu.Add(tptome);
                #endregion

                #region TeleportMe
                var tpme = new MenuItem("Téléporter sur le joueur", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        Client.Position = (_playerSelected.Client.Position);
                        Client.Dimension = (_playerSelected.Client.Dimension);
                        Client.SendNotificationSuccess($"vous venez d'être téléporté sur {_playerSelected.Identite.Name}");
                    }
                };
                mainMenu.Add(tpme);
                #endregion

                #region Kill
                var killitem = new MenuItem("Kill", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        client.SendNotificationSuccess($"Vous venez de tuer {_playerSelected.Identite.Name}.");
                        _playerSelected.Client.Health = 0;
                    }
                };
                mainMenu.Add(killitem);
                #endregion

                #region Kick
                var kickitem = new MenuItem("Kick le joueur", "", "", true);
                kickitem.SetInput("", 99, InputType.Text);
                kickitem.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    client.SendNotificationSuccess($"Vous venez de kick {_playerSelected.Identite.Name}.");
                    _playerSelected.Client.Kick(menuItem.InputValue);
                };
                mainMenu.Add(kickitem);
                #endregion

                #region Ban
                var banitem = new MenuItem("Ban le joueur", "", "", true);
                banitem.SetInput("", 99, InputType.Text);
                banitem.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    if (string.IsNullOrEmpty(menuItem.InputValue)) 
                        return;

                    client.SendNotificationSuccess($"Vous venez de ban {_playerSelected.Identite.Name}.");
                    BanManager.BanPlayer(_playerSelected.Client, menuItem.InputValue, new DateTime(2031, 1, 1));
                    // TODO ajouter contacte API
                };
                mainMenu.Add(banitem);
                #endregion

                #region Money
                var moneyitem = new MenuItem("Give de l'argent", "", "ID_GiveMoney", true);
                moneyitem.SetInput("", 10, InputType.UFloat, true);
                moneyitem.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    double money = 0;

                    if (double.TryParse(menuItem.InputValue, out money) && money > 0)
                    {
                        _playerSelected.AddMoney(money);
                        client.SendNotificationSuccess($"Vous venez de donner {money} à {_playerSelected.Identite.Name}");
                    }
                };
                mainMenu.Add(moneyitem);
                #endregion

                #region Remove Money
                var delmoneyitem = new MenuItem("Retirer de l'argent", "", "ID_RemoveMoney", true);
                delmoneyitem.SetInput("", 10, InputType.UFloat, true);
                delmoneyitem.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    double money = 0;

                    if (double.TryParse(menuItem.InputValue, out money) && money > 0)
                    {
                        if (_playerSelected.HasMoney(money))
                            client.SendNotificationSuccess($"Vous venez de retirer ${money} à {_playerSelected.Identite.Name}");
                    }
                };
                mainMenu.Add(delmoneyitem);
                #endregion

                #region NoClip
                var noclipitem = new MenuItem("NoClip", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        _playerSelected.Client.Emit("ToggleNoclip");
                    }
                };
                mainMenu.Add(noclipitem);
                #endregion

                #region Revive
                var resuitem = new MenuItem("Réanimer le joueur", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        _playerSelected.Client.Revive();
                    }
                };
                mainMenu.Add(resuitem);
                #endregion

                #region Spawn Perm
                var spawnPerm = new MenuItem("Spawn véhicule", "Spawn un véhicule avec le nom rentré.", "ID_SpawnVehPerm", true);
                spawnPerm.SetInput("", 30, InputType.Text);
                spawnPerm.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
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

                        Rotation rot = _playerSelected.Client.Rotation;
                        VehicleHandler vehicle = VehiclesManager.SpawnVehicle(_playerSelected.Client.GetSocialClub(), hash, _playerSelected.Client.Position, new Rotation(rot.Pitch, rot.Roll, -rot.Yaw), fuel: 100, fuelMax: 100, spawnVeh: false);

                        if (vehicle != null)
                        {
                            vehicle.LockState = VehicleLockState.Unlocked;
                            _playerSelected.Client.SetPlayerIntoVehicle(vehicle);
                            vehicle.VehicleData.InsertVehicle();
                            _playerSelected.ListVehicleKey.Add(new VehicleKey(manifest.DisplayName, vehicle.NumberplateText));
                            //LogManager.Log($"~r~[ADMIN]~w~ {client.Name} a spawn le véhicule {_vehs.Model} {_vehs.Plate}");
                        }
                        else
                            client.SendNotificationError("Il y a une erreur avec le véhicule demandé.");
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError($"ADMIN SPAWN VEHICLE PERSISTANT: {ex}");
                    }
                };
                mainMenu.Add(spawnPerm);
                #endregion

                #region Delete Vehicle Perm
                var deletepermvehitem = new MenuItem("Supprimer le véhicule (PERM).", "~r~ATTENTION CECI LE RETIRE DE LA BASE DE DONNÉES!", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        VehicleHandler vehicle = (client.GetNearestVehicle(5))?.GetVehicleHandler();
                        Task.Run(async () =>
                        {
                            if (vehicle == null)
                                client.SendNotificationError("Aucun véhicule a proximité");
                            else if (await vehicle.DeleteAsync(true))
                                client.SendNotificationSuccess($"Véhicule ~r~{vehicle.VehicleData.Plate}~w~ supprimé...");
                            else
                                client.SendNotificationError($"Erreur de suppression du véhicule");
                        });
                    }
                };

                mainMenu.Add(deletepermvehitem);
                #endregion

                #region Delete Vehicle
                var deletevehitem = new MenuItem("Supprimer le véhicule.", "", "", true)
                {
                    OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                    {
                        VehicleHandler vehicle = (client.GetNearestVehicle(5))?.GetVehicleHandler();
                        Task.Run(async () =>
                        {
                            if (vehicle == null)
                                client.SendNotificationError("Aucun véhicule a proximité");
                            else if (await vehicle.DeleteAsync(false))
                                client.SendNotificationSuccess($"Véhicule ~r~{vehicle.VehicleData.Plate}~w~ supprimé...");
                            else
                                client.SendNotificationError($"Erreur de suppression du véhicule");
                        });
                    }
                };

                mainMenu.Add(deletevehitem);
                #endregion

                #region AddItem
                var addItem = new MenuItem("Donner un objet", "", "", true)
                {
                    OnMenuItemCallback = GiveItemMenu
                };
                mainMenu.Add(addItem);
                #endregion

                #region Time Change All
                spawnPerm = new MenuItem("Changer l'heure All", "Exemple de format valide 16:00", "ID_SetTime", true);
                spawnPerm.SetInput("", 30, InputType.Text);
                spawnPerm.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    try
                    {
                        string name = menuItem.InputValue;

                        if (string.IsNullOrEmpty(name))
                            return;


                        string[] time = name.Split(':');

                        GameMode.Instance.Time.Hours = Convert.ToInt32(time[0]);
                        GameMode.Instance.Time.Minutes = Convert.ToInt32(time[1]);

                        Alt.EmitAllClients("SetTime", GameMode.Instance.Time.Hours, GameMode.Instance.Time.Minutes);
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError($"World Time Change All: {ex}");
                    }
                };
                mainMenu.Add(spawnPerm);
                #endregion

                #region Time Change Local
                spawnPerm = new MenuItem("Changer l'heure Local", "Exemple de format valide 16:00", "ID_SetTime", true);
                spawnPerm.SetInput("", 30, InputType.Text);
                spawnPerm.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    try
                    {
                        string name = menuItem.InputValue;

                        if (string.IsNullOrEmpty(name))
                            return;


                        string[] time = name.Split(':');

                        GameMode.Instance.Time.Hours = Convert.ToInt32(time[0]);
                        GameMode.Instance.Time.Minutes = Convert.ToInt32(time[1]);

                        client.Emit("SetTime", GameMode.Instance.Time.Hours, GameMode.Instance.Time.Minutes);
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError($"World Time Change Local: {ex}");
                    }
                };
                mainMenu.Add(spawnPerm);
                #endregion

                #region Duplicate clé 

                spawnPerm = new MenuItem("Donner une clé", "Donne une clé de véhicule, avec la plaque.", "ID_Generatekey", true);
                spawnPerm.SetInput("", 30, InputType.Text);
                spawnPerm.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    try
                    {
                        string name = menuItem.InputValue;

                        if (string.IsNullOrEmpty(name))
                            return;


                        VehicleData vehicleData = VehiclesManager.GetVehicleDataWithPlate(name);

                        if (vehicleData != null && vehicleData.Vehicle != null)
                        {
                            var vehicle = vehicleData.Vehicle;

                            _playerSelected.AddKey(vehicle, "Clé de " + vehicle.VehicleManifest.Name);
                            _playerSelected.Client.DisplayHelp("Un admin vous a donné:\n" + "Clé de " + vehicle.VehicleManifest.Name);
                            client.DisplayHelp("Vous avez donné:\n" + "Clé de " + vehicle.VehicleManifest.Name);
                        }
                        else
                        {
                            client.SendNotificationError($"véhicule inconnu : {name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError($"ADMIN CREATE KEY PERSISTANT: {ex}");
                    }
                };
                mainMenu.Add(spawnPerm);
                #endregion


                #region Remove a car

                spawnPerm = new MenuItem("Supprimer un véhicule (Nécéssite plaque)", "Supprimer un véhicule DEFINiTIVEMENT en utilsant sa plaque", "ID_PermDeleteWithPlate", true);
                spawnPerm.SetInput("", 30, InputType.Text);
                spawnPerm.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    try
                    {
                        string name = menuItem.InputValue;

                        if (string.IsNullOrEmpty(name))
                            return;


                        VehicleHandler vehicle = VehiclesManager.GetVehicleDataWithPlate(name)?.Vehicle;

                        Task.Run(async () =>
                        {
                            if (vehicle == null)
                                client.DisplayHelp("Le véhicule n'existe pas!");
                            else if (await vehicle.DeleteAsync(true))
                                client.DisplayHelp($"Le véhicule {vehicle.VehicleManifest.DisplayName} a été supprimé en base de donnée.");
                            else
                                client.DisplayHelp("Le véhicule n'a pas pu être supprimé, une erreur en base donnée peut-être :x");
                        });
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError($"(PlayerHandler.Admin.OpenMenuAdmin) {ex}");
                    }
                };
                mainMenu.Add(spawnPerm);
                #endregion

                #region Get Vehicle Infos

                spawnPerm = new MenuItem("Information Véhicule (Nécéssite plaque)", "Récupère les informations d'un véhicule", "ID_VehicleInfo", true);
                spawnPerm.SetInput("", 30, InputType.Text);
                spawnPerm.OnMenuItemCallback = (IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex) =>
                {
                    Task.Run(async ()=> await VehicleCommands.VehicleInfo(client, new[] {  menuItem.InputValue }));
                };
                mainMenu.Add(spawnPerm);
                #endregion

                #region DEBUG CreateObject
                if (GameMode.IsDebug)
                {
                    spawnPerm = new MenuItem("DEBUG: Object Hand", "Tester les objets dans sa main", "ID_ObjectHand", true);
                    spawnPerm.SetInput("", 30, InputType.Text);
                    spawnPerm.OnMenuItemCallback = DEBUG_ObjectHand;
                    mainMenu.Add(spawnPerm);
                }
                #endregion
            }

            #region Fin
            mainMenu.OpenMenu(Client);
            #endregion
        }

        private void OnFinalize(IPlayer client, Menu menu)
        {
            /*
            if (_playerSelected.Client != null && _playerSelected.Client.Exists)
                _playerSelected.Client.ResetSharedData("AC_Immunity");
            */
            _playerSelected = null;
        }
        #endregion

        #region CallBacks
        private void ChoisePlayer(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var secondMenu = new Menu("", "Admin Menu", "Choisissez un joueur :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Garage)
            {
                ItemSelectCallback = ChoisePlayerCallback
            };

            List<PlayerHandler> players = PlayerManager.GetPlayersList();

            var test = players.OrderBy(pa => pa.Identite.Name).ToList();
            foreach (PlayerHandler player in test)
            {
                if (player != null && player.Client.Exists)
                {
                    string description = "";
                    var item = new MenuItem(player.Identite.Name, description, executeCallback: true)
                    {
                        OnMenuItemCallback = (IPlayer cClient, Menu cMenu, IMenuItem cMenuItem, int _itemIndex) =>
                       {
                           PlayerHandler playerSelected = test[_itemIndex];

                           if(!playerSelected.Client.Exists)
                               return;

                           OpenAdminMenu(playerSelected);
                       }
                    };

                    secondMenu.Add(item);
                }
            }

            secondMenu.OpenMenu(Client);
        }

        private void GiveItemMenu(IPlayer client, Menu oldMenu, IMenuItem oldMenuItem, int itemIndex)
        {
            PlayerHandler targetClient = _playerSelected;
            if (client == null || !client.Exists)
                return;

            Menu menu = new Menu("ID_AdminGiveItem", "Donner à un joueur", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true)
            {
                ItemSelectCallback = (IPlayer Player, Menu itemMenu, IMenuItem menuItem, int itemMenuIndex) =>
                {
                    try {

                        Item model = menuItem.GetData("Item");
                        if(targetClient.InventoryIsFull(model.weight)) {
                            client.DisplayHelp("L'inventaire du joueur est plein !");
                            return;
                        }

                        if(model.id == ItemID.Phone) {
                            targetClient.AddItem(new PhoneItem(ItemID.Phone, "Téléphone", "", PhoneManager.GeneratePhone(), 0, true, true, false, true, false), 1);
                        } else {
                            targetClient.AddItem(model, 1);
                        }
                        targetClient.Client.DisplayHelp("On vous a donné " + model.name + "\nVous en avez " + targetClient.CountItem(model.id) + " désormais");
                        client.DisplayHelp("On vous a donné " + model.name + "\nVous en avez " + targetClient.CountItem(model.id) + " désormais");
                    } catch(Exception ex) {

                        Alt.Server.LogError("GiveItemAdmin | Error on parsing string to int |" + ex.Data);
                    }
                }
            };


            foreach (Models.Item item in LoadItem.ItemsList)
            {
                MenuItem listItem = new MenuItem(item.name, "", item.id.ToString(), true, rightLabel: "1");
                listItem.SetData("Item", item);
                menu.Add(listItem);
            }


            menu.Add(new MenuItem("~g~Valider", "", "Validate", true));

            menu.OpenMenu(client);
        }

        private void DEBUG_ObjectHand(IPlayer Client, Menu Menu, IMenuItem MenuItem, int ItemIndex)
        {
            string name = MenuItem.InputValue;

            if (string.IsNullOrEmpty(name))
                return;
            Client.Emit("DEBUG_createObject", Alt.Hash(name));

            Menu.ClearItems();

            Menu.SubTitle = "Gestion de l'objet";
            Menu.BackCloseMenu = true;

            MenuItem item = null;

            for(int i = 0; i <6; i++)
            {
                string axe = (i <= 2 ? (i == 2) ? "Z" : (i == 1 ? "X" : "Y") : (i == 5) ? "Z" : (i == 3 ? "X" : "Y"));
                string add = (i <= 2) ? "Ajouter" : "Enlever";
                item = new MenuItem(add + " 0.01 " + axe, "0: X 1: Y 2: Z", "ID_add" + axe, true)
                {
                    OnMenuItemCallback = (IPlayer Player, Menu itemMenu, IMenuItem menuItem, int itemMenuIndex) => Player.Emit($"DEBUG_update{axe}Object", (add == "Ajouter") ? 0.01 : -0.01)
                };
                Menu.Add(item);
            }
            
            for(int i = 0; i <6; i++)
            {
                string axe = (i <= 2 ? (i == 2) ? "RZ" : (i == 1 ? "RX" : "RY") : (i == 5) ? "RZ" : (i == 3 ? "RX" : "RY"));
                string add = (i <= 2) ? "Ajouter" : "Enlever";
                item = new MenuItem(add + " 1 " + axe, "0: X 1: Y 2: Z", "ID_add" + axe, true)
                {
                    OnMenuItemCallback = (IPlayer Player, Menu itemMenu, IMenuItem menuItem, int itemMenuIndex) => Player.Emit($"DEBUG_update{axe}Object", (add == "Ajouter") ? 1 : -1)
                };
                Menu.Add(item);
            }


            Menu.OpenMenu(Client);
        }

        private void ChoisePlayerCallback(IPlayer client, Menu menu, IMenuItem menuItem, int _itemIndex)
        {
            if (menuItem == null)
                OpenAdminMenu();
        }
        #endregion
    }
}
