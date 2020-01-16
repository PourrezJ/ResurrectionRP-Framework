using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.XMenuManager;
using System.Numerics;
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
        private IColshape colshape;

        public DeadPlayer(IPlayer player, IEntity killer, uint weapon)
        {
            Victime = player;
            Killer = killer;
            Weapon = weapon;

            marker = Marker.CreateMarker(MarkerType.UpsideDownCone, player.Position);
            colshape = ColshapeManager.CreateCylinderColshape(player.Position - new Position(0, 0, 1), 2f, 2f);
            colshape.OnPlayerEnterColshape += OnPlayerEnterColshape;
            colshape.OnPlayerLeaveColshape += OnPlayerExitColshape;
        }

        private void OnPlayerEnterColshape(IColshape colShape, IPlayer client)
        {
            if (client == Victime)
                return;

            PlayerHandler ph = client.GetPlayerHandler();

            XMenu menu = new XMenu("");
            menu.Callback = CallBack;

            if (Factions.FactionManager.IsMedic(client) && ph.HasItemID(ItemID.Defibrilateur))
                menu.Add(new XMenuItem("RPC", "Réanimer la victime", "ID_Reanimate", XMenuItemIcons.HEART_SOLID, false));

            menu.Add(new XMenuItem("Fouiller les poches", "", "ID_Poche", XMenuItemIcons.HANDS_SOLID));

            if (Victime.GetPlayerHandler().BagInventory != null)
                menu.Add(new XMenuItem("Fouiller le sac à dos", "", "ID_Bag", XMenuItemIcons.HANDS_SOLID));


            menu.Add(new XMenuItem("Prendre l'argent", "", "ID_GetMoney", XMenuItemIcons.MONEY_BILL_SOLID));
            menu.Add(new XMenuItem("Regarder la carte d'identité", "", "ID_Identite", XMenuItemIcons.ID_CARD_ALT_SOLID));
            menu.Add(new XMenuItem("Envoyer en soin intensif", "", "ID_Intensif", XMenuItemIcons.SKULL_SOLID));

            menu.OpenXMenu(client);
        }

        private void CallBack(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            if (client == Victime)
                return;

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
                                    client.SendNotification("Le défibrillateur est déchargé.");
                                    ph.PocketInventory.Delete(defibrilators[InventoryTypes.Pocket][0], 1);
                                }
                            }
                            else if (defibrilators.ContainsKey(InventoryTypes.Bag))
                            {
                                Defibrilator defribrilator = defibrilators[InventoryTypes.Bag][0].Item as Defibrilator;
                                if (defribrilator.Usage >= 3)
                                {
                                    client.SendNotification("Le défibrillateur est déchargé.");
                                    ph.BagInventory.Delete(defibrilators[InventoryTypes.Bag][0], 1);
                                }
                            }
                            Victime.Revive(125);
                            vh.UpdateFull();
                            client.SendNotificationSuccess("Vous avez réanimé le patient.");
                        }
                    }
                    break;

                case "ID_Poche":
                    Rob(client, false);
                    break;

                case "ID_Bag":
                    Rob(client, true);
                    break;

                case "ID_GetMoney":
                    var rob = vh.Money;
                    if (vh.HasMoney(rob))
                        ph.AddMoney(rob);

                    ph.UpdateFull();
                    vh.UpdateFull();

                    Victime.SendNotification("On vient de vous dérober l'argent que vous aviez sur vous.");
                    client.SendNotification($"Vous venez de dérober ${rob}");
                    break;

                case "ID_Identite":
                    client.SendNotification($"Nom: {vh.Identite.LastName} <br/>Prenom: {vh.Identite.FirstName}<br/>Age: {vh.Identite.Age}");
                    break;

                case "ID_Intensif":
                    if (!ph.HasItemID(ItemID.Knife))
                    {
                        client.SendNotificationError("Il vous faut un couteau pour faire cette action.");
                        return;
                    }

                    Victime.Revive(200, new Vector3(308.2974f, -567.4647f, 43.29008f));
                    vh.UpdateFull();
                    break;
            }
        }

        public void Rob(IPlayer robber, bool bag)
        {
            var ph = robber.GetPlayerHandler();

            if (ph == null)
                return;

            var vh = Victime.GetPlayerHandler();

            if (vh == null)
                return;

            XMenuManager.XMenuManager.CloseMenu(robber);
            Victime.SendNotification("Quelqu'un fouille vos poches");
            var invmenu = new Inventory.RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, (bag) ? vh.BagInventory : vh.PocketInventory);
            invmenu.OnMove += (p, m) =>
            {
                ph.UpdateClothing();
                vh.UpdateClothing();

                ph.UpdateFull();
                vh.UpdateFull();
            };

            invmenu.OnClose += (p, m) =>
            {
                ph.UpdateFull();
                vh.UpdateFull();
            };
            invmenu.OpenMenu(robber);
        }

        private void OnPlayerExitColshape(IColshape colShape, IPlayer client)
        {
            XMenuManager.XMenuManager.CloseMenu(client);
        }

        public void TakeCall()
        {
            Taken = true;
        }

        public void Remove()
        {
            marker?.Destroy();
            colshape?.Delete();
            PlayerManager.DeadPlayers.Remove(this);
        }
    }
}
