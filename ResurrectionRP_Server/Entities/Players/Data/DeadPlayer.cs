﻿using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.XMenuManager;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public class DeadPlayer
    {
        public IPlayer Victime { get; private set; }
        public bool Taken { get; private set; }
        public IEntity Killer { get; private set; }
        public uint Weapon { get; private set; }


        private Marker marker;
        private IColShape colshape;

        public DeadPlayer(IPlayer player, IEntity killer, uint weapon)
        {
            Victime = player;
            Killer = killer;
            Weapon = weapon;

            marker = Marker.CreateMarker(MarkerType.UpsideDownCone, player.Position);
            colshape = Alt.CreateColShapeCylinder(player.Position - new Position(0, 0, 1), 1f, 1f);
            colshape.SetOnPlayerEnterColShape(OnPlayerEnterColshape);
            colshape.SetOnPlayerLeaveColShape(OnPlayerExitColshape);
        }

        private void OnPlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            PlayerHandler ph = client.GetPlayerHandler();

            XMenu menu = new XMenu("");
            menu.Callback = CallBack;

            if (Factions.FactionManager.IsMedic(client))
                menu.Add(new XMenuItem("RPC", "Réanimer la victime", "ID_Reanimate", XMenuItemIcons.HEART_SOLID, false));

            menu.Add(new XMenuItem("Fouiller les poches", "", "ID_Poche", XMenuItemIcons.GET_POCKET_BRAND));
            menu.Add(new XMenuItem("Prendre l'argent", "", "ID_GetMoney", XMenuItemIcons.MONEY_BILL_SOLID));
            menu.Add(new XMenuItem("Regarder la carte d'identité", "", "ID_Identite", XMenuItemIcons.ID_CARD_ALT_SOLID));
            menu.Add(new XMenuItem("Envoyer en soin intensif", "", "ID_Intensif", XMenuItemIcons.SKULL_SOLID));

            menu.OpenXMenu(client);
        }

        private void CallBack(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            var ph = client.GetPlayerHandler(); // Personne dans le colshape

            if (!Victime.Exists)
                return;

            var vh = Victime.GetPlayerHandler(); // Victime Handler

            switch (menuItem.Id)
            {
                case "ID_Reanimate":
                    if (Victime.IsDead && ph.HasItemID(ItemID.Defibrilateur))
                    {
                        var defibrilators = ph.GetStacksItems(ItemID.Defibrilateur);
                        if (defibrilators.Count > 0)
                        {
                            if (defibrilators.ContainsKey(InventoryTypes.Pocket))
                            {
                                Defibrilator defribrilator = defibrilators[InventoryTypes.Pocket][0].Item as Defibrilator;
                                if (defribrilator.Usage >= 3)
                                {
                                    client.SendNotification("Le défibrillateur est décharger.");
                                    ph.PocketInventory.Delete(defibrilators[InventoryTypes.Pocket][0], 1);
                                }
                            }
                            else if (defibrilators.ContainsKey(InventoryTypes.Bag))
                            {
                                Defibrilator defribrilator = defibrilators[InventoryTypes.Bag][0].Item as Defibrilator;
                                if (defribrilator.Usage >= 3)
                                {
                                    client.SendNotification("Le défibrillateur est décharger.");
                                    ph.BagInventory.Delete(defibrilators[InventoryTypes.Bag][0], 1);
                                }
                            }
                            Victime.Revive(125);
                            marker.Destroy();
                            colshape.Remove();
                            client.SendNotificationSuccess("Vous avez réanimé le patient.");
                        }
                    }
                    break;

                case "ID_Poche":      
                    var invmenu = new Inventory.RPGInventoryMenu(vh.PocketInventory, vh.OutfitInventory, vh.BagInventory);
                    invmenu.OnMove += (p, m) =>
                    {
                        ph.UpdateFull();
                        vh.UpdateFull();
                        return Task.CompletedTask;
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        ph.UpdateFull();
                        vh.UpdateFull();
                        return Task.CompletedTask;
                    };
                    Task.Run(async ()=> await invmenu.OpenMenu(client));
                    break;

                case "ID_GetMoney":
                    var rob = ph.Money;
                    if (ph.HasMoney(ph.Money))
                        vh.AddMoney(rob);

                    ph.UpdateFull();
                    vh.UpdateFull();
                    break;

                case "ID_Identite":
                    client.SendNotification($"Nom: {vh.Identite.LastName} <br/>Prenom: {vh.Identite.FirstName}<br/>Age: {vh.Identite.Age}");
                    break;

                case "ID_Intensif":
                    marker.Destroy();
                    colshape.Remove();
                    PlayerManager.Resurrect(Victime);
                    break;
            }
        }

        private void OnPlayerExitColshape(IColShape colShape, IPlayer client)
        {
            XMenuManager.XMenuManager.CloseMenu(client);
        }

        public void TakeCall()
        {
            Taken = true;
        }

        public void Revive()
        {
            marker.Destroy();
            colshape.Remove();
        }
    }
}
