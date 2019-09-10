using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.XMenuManager;
using ResurrectionRP_Server.Menus;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Factions
{

    public partial class ONU
    {
        #region Infirmiere
        private async Task OnNPCInteract(IPlayer client, Ped npc)
        {
            AcceptMenu healmenu = await AcceptMenu.OpenMenu(client, "Infirmière", "Voulez-vous être soigné?", rightlabel: $"${healprice}");
            healmenu.AcceptMenuCallBack = (async (IPlayer c, bool reponse) =>
            {
                if (reponse)
                {
                    if (await c.GetPlayerHandler().HasBankMoney(healprice, "Soin Hospital"))
                    {
                        c.Health = 100;
                        c.GetPlayerHandler().PlayerSync.Injured = false;
                        c.SendNotificationSuccess("Voilà qui est fait!");
                    }
                    else
                    {
                        c.SendNotificationError("Désolé Mr mais la banque refuse le paiement de vos soins.");
                    }
                }
                else
                {
                    c.SendNotification("Ne me faites pas perdre mon temps alors!");
                }
            });
        }
        #endregion

        #region Interaction
        public override async Task<XMenu> InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            var playerHandler = client.GetPlayerHandler();
            xmenu.SetData("Player", target);

            xmenu.Add(new XMenuItem("Facturer", "Envoyer une facture au patient", "ID_SendInvoice", XMenuItemIcons.FILE_INVOICE_DOLLAR_SOLID, false));

            if (target.IsDead() && playerHandler.GetStacksItems(Models.InventoryData.ItemID.Defibrilateur).Count > 0)
            {
                xmenu.Add(new XMenuItem("RPC", "Réanimer la victime", "ID_Reanimate", XMenuItemIcons.HEART_SOLID, false));
            }

            if (playerHandler.HasItemID(ItemID.Bandages))
                xmenu.Add(new XMenuItem("Bandage", "Appliquer un bandage au patient", "ID_Bandage", XMenuItemIcons.HEART_SOLID, false));

            if (playerHandler.HasItemID(ItemID.KitSoin))
                xmenu.Add(new XMenuItem("Kit de Soin", "Appliquer un kit de soin au patient", "ID_KitSoin", XMenuItemIcons.HEART_SOLID, false));

            xmenu.Callback += OnInteractCallBack;

            return await base.InteractPlayerMenu(client, target, xmenu);
        }

        private async Task OnInteractCallBack(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");
            var ph = client.GetPlayerHandler();
            if (_target == null || ph == null) return;

            var healthActual = await _target.GetHealthAsync();
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
                        await _target.Revive();

                        client.SendNotificationSuccess("Vous avez réanimé le patient.");
                    }

                    break;

                case "ID_KitSoin":
                    if (ph.DeleteOneItemWithID(ItemID.KitSoin))
                    {
                        if ((healthActual += 75) > 100)
                            await _target.SetHealthAsync(100);
                        else
                            await _target.SetHealthAsync((ushort)(healthActual + 75));
                        client.SendNotificationSuccess("Vous avez appliqué un kit de soin au patient.");
                    }
                    break;

                case "ID_Bandage":
                    if (ph.DeleteOneItemWithID(ItemID.Bandages))
                    {
                        if ((healthActual + 5) > 100)
                            await _target.SetHealthAsync(100);
                        else
                            await _target.SetHealthAsync(healthActual += 5);
                        client.SendNotificationSuccess("Vous avez appliqué un bandage au patient.");
                    }
                    break;
            }
        }
        #endregion
    }
}
