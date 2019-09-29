using System;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using XMenuItem = ResurrectionRP_Server.XMenuManager.XMenuItem;
using XMenu = ResurrectionRP_Server.XMenuManager.XMenu;
using XMenuItemIcons = ResurrectionRP_Server.XMenuManager.XMenuItemIcons;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Factions;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        IPlayer TargetClient;
        PlayerHandler TargetHandler;

        public async Task OpenXtremPlayer(IPlayer targetClient)
        {
            if (!await targetClient.ExistsAsync())
                return;

            TargetClient = targetClient;
            TargetHandler = targetClient.GetPlayerHandler();
            if (TargetHandler == null) 
                return;

            XMenu xmenu = new XMenu("PlayerMenu");
            xmenu.CallbackAsync = PlayerXMenuCallback;

            xmenu.Add(new XMenuItem("Passeport", "Montrer son passeport", "ID_ShowPassport", XMenuItemIcons.PASSPORT_SOLID, false));
            xmenu.Add(new XMenuItem("Licences", "Montrer ses permis", "ID_Licences", XMenuItemIcons.HAND_PAPER_SOLID, false));
            xmenu.Add(new XMenuItem("Donner un objet", "Donner un objet de votre inventaire", "ID_GiveItem", XMenuItemIcons.HANDS_SOLID, false));

            var givemoney = new XMenuItem("Donner de l'argent", "Donner de l'argent", "ID_GiveMoney", XMenuItemIcons.MONEY_BILL_SOLID, false);
            givemoney.SetInput("0", 4, InputType.Number);
            xmenu.Add(givemoney);

            if (GetStacksItems(Models.InventoryData.ItemID.Handcuff).Count > 0)
                xmenu.Add(new XMenuItem($"{((!TargetHandler.IsCuff()) ? "Menotté" : "Démenotté")}", "", "ID_Handcuff", XMenuItemIcons.HANDS_SOLID, false));

            if (TargetHandler.IsCuff())
            {
                xmenu.Add(new XMenuItem("Fouiller", "Fouiller la personne", "ID_SearchInventory", XMenuItemIcons.ACCESSIBILITY, false));
                xmenu.Add(new XMenuItem("Embarquer", "Embarquer la personne", "ID_PutIntoCar", XMenuItemIcons.DIRECTIONS_CAR, false));
            }

            if (StaffRank >= Utils.Enums.AdminRank.Moderator)
                xmenu.Add(new XMenuItem("ADMIN", $"{TargetHandler.PID} {TargetHandler.Identite.Name}", "ID_Admin", XMenuItemIcons.SETTINGS, false));

            if (FactionManager.IsLSCustom(Client) || FactionManager.IsLspd(Client) || FactionManager.IsMedic(Client) || FactionManager.IsNordiste(Client) || FactionManager.IsDock(Client))
            {
                xmenu.Add(new XMenuItem("Faction", "", "ID_Faction", XMenuItemIcons.ID_BADGE_SOLID, false));
            }
            

            xmenu.OpenXMenu(Client);
        }

        private async Task PlayerXMenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            var ph = client.GetPlayerHandler();
            if (ph == null)
                return;

            switch (menuItem.Id)
            {
                case "ID_GiveItem":
                    var rpg = new Inventory.RPGInventoryMenu(PocketInventory, OutfitInventory, BagInventory, null, false, TargetClient);
                    await rpg.OpenMenu(client);
                    break;

                case "ID_Licences":
                    string licenceStr = "";
                    foreach (var licence in ph.Licenses)
                    {
                        if (licence.Type.ToString() != "")
                            licenceStr += licence.Type + " " + licence.Point + "\n";
                    }
                    if (licenceStr == "")
                        licenceStr = "La personne n'a aucune license!";
                    TargetClient.SendNotification(licenceStr);
                    break;

                case "ID_ShowPassport":
                    TargetClient.SendNotification($"Nom: {ph.Identite.LastName} <br/>Prenom: {ph.Identite.FirstName}<br/>Age: {ph.Identite.Age}");
                    break;

                case "ID_GiveMoney":

                    double money = 0;
                    try
                    {
                        money = Convert.ToDouble(menuItem.InputValue);
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError($"PlayerXMenuCallback | ID_GiveMoney | {client.GetSocialClub()} | {menuItem.InputValue} " + ex);
                    }

                    if (money == 0)
                        return;

                    if (HasMoney(money))
                    {
                        TargetHandler.AddMoney(money);
                        Client.SendNotificationSuccess($"Vous avez donné la somme de ${money}.");
                        TargetClient.SendNotificationSuccess($"On vous a donné la somme de ${money}.");
                    }
                    else
                    {
                        Client.SendNotificationError("Vous n'avez pas la somme demandée.");
                    }

                    break;
                case "ID_SearchInventory":
                    TargetClient.SendNotification("Quelqu'un fouille vos poches");
                    var invmenu = new Inventory.RPGInventoryMenu(TargetHandler.PocketInventory, TargetHandler.OutfitInventory, TargetHandler.BagInventory);
                    invmenu.OnMove += (p, m) =>
                    {
                        UpdateFull();
                        return Task.CompletedTask;
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        UpdateFull();
                        TargetHandler.UpdateFull();
                        return Task.CompletedTask;
                    };
                    await invmenu.OpenMenu(client);
                    break;

                case "ID_Handcuff":
                    await AltAsync.Do(() =>
                    {
                        bool cuffed = TargetHandler.IsCuff();
                        TargetHandler?.SetCuff(!cuffed);
                    });
                    break;

                case "ID_Admin":
                    OpenXtremAdmin();
                    break;

                case "ID_PutIntoCar":
                    if (TargetHandler.IsCuff())
                    {
                        IVehicle vehicle = Vehicles.VehiclesManager.GetNearestVehicle(Client);

                        if (vehicle != null)
                        {
                            if (await vehicle.GetLockStateAsync() == AltV.Net.Enums.VehicleLockState.Locked)
                                Client.SendNotificationError("Le véhicule est fermé");
                            else
                            {
                                VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(vehicle.Model);

                                if (manifest != null)
                                    TargetClient.EmitLocked("TrySetPlayerIntoVehicle", vehicle);
                                else
                                    Client.SendNotificationError("Impossible de le mettre dans ce véhicule");
                            }
                        }
                        else
                            Client.SendNotificationError("Aucun véhicule proche de vous");
                    }
                    break;
                
                case "ID_Faction": 
                    menu.ClearItems();
                    FactionManager.AddFactionTargetMenu(Client, TargetClient, menu);
                    menu.OpenXMenu(client);
                    break;
                
            }
        }
    }
}
