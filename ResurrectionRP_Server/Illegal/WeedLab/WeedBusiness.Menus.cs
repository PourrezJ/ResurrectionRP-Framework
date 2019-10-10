using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Illegal.WeedLab;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.XMenuManager;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Illegal
{
    public partial class WeedBusiness
    {
        #region Menu
        public void OpenMenuGrow(IPlayer client, WeedZone zone)
        {
            XMenu xmenu = new XMenu("ID_Weed");
            xmenu.SetData("Zone", zone);
            xmenu.Callback = GrowZoneMenuCallback;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (zone.GrowingState == 0 && !zone.Plant)
            {
                /*
                if (GameMode.Instance.FactionManager.Lspd.ServicePlayerList.Count < 2)
                {
                    client.SendNotificationError("[HRP] Pas assez de miliciens de présent sur le serveur.");
                    return;
                }*/

                if (!ph.HasItemID(ItemID.GSkunk) && !ph.HasItemID(ItemID.GSkunk) && ph.HasItemID(ItemID.GSkunk) && ph.HasItemID(ItemID.GSkunk))
                {
                    client.SendNotificationError("Vous n'avez pas de graine sur vous.");
                    return;
                }

                if (ph.HasItemID(ItemID.GSkunk))
                {
                    xmenu.Add(new XMenuItem("Planter Skunk", "Planter vos graines de Skunk", "ID_SeedSkunk", XMenuItemIcons.SEEDLING_SOLID));
                }

                if (ph.HasItemID(ItemID.GPurple))
                {
                    xmenu.Add(new XMenuItem("Planter Purple", "Planter vos graines de Purple", "ID_SeedPurple", XMenuItemIcons.SEEDLING_SOLID));
                }

                if (ph.HasItemID(ItemID.GOrange))
                {
                    xmenu.Add(new XMenuItem("Planter OrangeBud", "Planter vos graines d'orange bud", "ID_SeedOrange", XMenuItemIcons.SEEDLING_SOLID));
                }

                if (ph.HasItemID(ItemID.GWhite))
                {
                    xmenu.Add(new XMenuItem("Planter White Widow", "Planter vos graines de White Widow", "ID_SeedWhite", XMenuItemIcons.SEEDLING_SOLID));
                }
            }

            if (ph.HasItemID(ItemID.Hydro) && zone.Spray == Spray.Off)
            {
                xmenu.Add(new XMenuItem("Installer l'hydroponie", "Relier les pots au système d'hydroponie", "ID_Hydro", XMenuItemIcons.BRANDING_WATERMARK));
            }

            if (zone.Plant)
            {
                xmenu.Add(new XMenuItem("Arosser", "Arroser les pieds fait monter l'hydratation", "ID_Tint", XMenuItemIcons.TINT_SOLID));
            }

            if (zone.GrowingState == StateZone.Stage3 && ph.HasItemID(ItemID.Secateur))
            {
                xmenu.Add(new XMenuItem("Récolter", $"Récolter vos pieds de {zone.SeedUsed.ToString()} et les mettre a sécher (10minutes).", "ID_Recolte", XMenuItemIcons.CUT_SOLID));
            }
            xmenu.OpenXMenu(client);
        }

        private void GrowZoneMenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            WeedZone zone = (WeedZone)menu.GetData("Zone");
            if (zone == null) return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            switch (menuItem.Id)
            {
                case "ID_SeedSkunk":
                    if (ph.DeleteOneItemWithID(ItemID.GSkunk))
                    {
                        zone.SeedUsed = SeedType.Skunk;
                        zone.Plant = true;
                    }
                    break;
                case "ID_SeedPurple":
                    if (ph.DeleteOneItemWithID(ItemID.GPurple))
                    {
                        zone.SeedUsed = SeedType.Purple;
                        zone.Plant = true;
                    }
                    break;
                case "ID_SeedOrange":
                    if (ph.DeleteOneItemWithID(ItemID.GOrange))
                    {
                        zone.SeedUsed = SeedType.Orange;
                        zone.Plant = true;
                    }
                    break;
                case "ID_SeedWhite":
                    if (ph.DeleteOneItemWithID(ItemID.GWhite))
                    {
                        zone.SeedUsed = SeedType.WhiteWidow;
                        zone.Plant = true;
                    }
                    break;
                case "ID_Hydro":
                    if (ph.DeleteOneItemWithID(ItemID.Hydro))
                    {
                        zone.Spray = Spray.On;
                        zone.Hydratation = 100;
                    }
                    break;
                case "ID_Tint":
                    if (zone.Hydratation + 10 <= 100)
                        zone.Hydratation += 10;
                    else zone.Hydratation = 100;
                    break;
                case "ID_Recolte":
                    Recolte(zone);
                    break;
            }

            LabelRefresh(zone);
            Task.Run(async () => await Update());

            if (menuItem.Id == "ID_Tint")
                menu.OpenXMenu(client);
            else
                RefreshClientInLabs(zone);
        }

        private void OnMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem.Id == "OpenInventory")
            {
                var ph = client.GetPlayerHandler();
                var inv = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, Inventory);
                inv.OnMove += async (cl, inventaire) =>
                {
                    ph.UpdateFull();
                    await Update();
                };
                menu.CloseMenu(client);
                inv.OpenMenu(client);
            }
        }

        public override void OnPlayerDisconnected(IPlayer client)
        {
            PlayersInside.Remove(client);
            base.OnPlayerDisconnected(client);
        }

        #endregion
    }
}
