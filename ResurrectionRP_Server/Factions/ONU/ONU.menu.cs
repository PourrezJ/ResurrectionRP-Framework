using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.XMenuManager;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Utils.Enums;
using System;

namespace ResurrectionRP_Server.Factions
{

    public partial class ONU
    {
        #region Infirmiere
        private void OnNPCInteract(IPlayer client, Ped npc)
        {
            AcceptMenu healmenu = AcceptMenu.OpenMenu(client, "Infirmière", "Voulez-vous être soigné?", rightlabel: $"${healprice}");

            healmenu.AcceptMenuCallBack = (IPlayer player, bool reponse) =>
            {
                if (reponse)
                {
                    PlayerHandler ph = player.GetPlayerHandler();

                    if (ph == null)
                        return;

                    if (player.GetPlayerHandler().HasBankMoney(healprice, "Soin Hospital", false))
                    {
                        ph.SetHealth(200);
                        ph.PlayerSync.Injured = false;
                        ph.UpdateInBackground();
                        ph.Client.SendNotificationSuccess("Voilà qui est fait!");
                    }
                    else
                        player.SendNotificationError("Désolé Mr mais la banque refuse le paiement de vos soins.");
                }
                else
                    player.SendNotification("Ne me faites pas perdre mon temps alors!");

                return;
            };
        }
        #endregion

        #region Interaction
        public override XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            try
            {
                var playerHandler = client.GetPlayerHandler();
                xmenu.SetData("Player", target);

                xmenu.Add(new XMenuItem("Facturer", "Envoyer une facture au patient", "ID_SendInvoice", XMenuItemIcons.FILE_INVOICE_DOLLAR_SOLID, false));

                if (target.IsDead && playerHandler.HasItemID(ItemID.Defibrilateur))
                {
                     xmenu.Add(new XMenuItem("RPC", "Réanimer la victime", "ID_Reanimate", XMenuItemIcons.HEART_SOLID, false));
                }

                if (playerHandler.HasItemID(ItemID.Bandages))
                    xmenu.Add(new XMenuItem("Bandage", "Appliquer un bandage au patient", "ID_Bandage", XMenuItemIcons.HEART_SOLID, false));

                if (playerHandler.HasItemID(ItemID.KitSoin))
                    xmenu.Add(new XMenuItem("Kit de Soin", "Appliquer un kit de soin au patient", "ID_KitSoin", XMenuItemIcons.HEART_SOLID, false));

                xmenu.Callback += OnInteractCallBack;
            }
            catch(Exception ex)
            {
                AltV.Net.Alt.Server.LogError(ex.ToString());
            }


            return base.InteractPlayerMenu(client, target, xmenu);
        }

        private void OnInteractCallBack(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            var ph = client.GetPlayerHandler();
            if (_target == null || ph == null) return;

            var healthActual = _target.Health;
            switch (menuItem.Id)
            {
                case "ID_Reanimate":

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
                        _target.Revive(125);

                        client.SendNotificationSuccess("Vous avez réanimé le patient.");
                    }

                    break;

                case "ID_KitSoin":
                    if (ph.DeleteOneItemWithID(ItemID.KitSoin))
                    {
                        if ((healthActual += 75) > 200)
                            _target.Health = 200;
                        else
                            _target.Health = (ushort)(healthActual + 75);
                        client.SendNotificationSuccess("Vous avez appliqué un kit de soin au patient.");
                    }
                    break;

                case "ID_Bandage":
                    if (ph.DeleteOneItemWithID(ItemID.Bandages))
                    {
                        if ((healthActual + 5) > 200)
                            _target.Health = 200;
                        else
                            _target.Health = healthActual += 5;
                        client.SendNotificationSuccess("Vous avez appliqué un bandage au patient.");
                    }
                    break;
            }
        }
        #endregion
    }
}
