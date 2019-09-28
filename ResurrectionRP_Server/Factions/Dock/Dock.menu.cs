using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResurrectionRP_Server.Factions
{
    public partial class Dock : Faction
    {
        #region Private fields
        private double _orderPrice;
        #endregion

        #region Importation menu
        public virtual void OpenImportationMenu(IPlayer client, List<DockItemData> importItems)
        {
            if (client == null || !client.Exists)
                return;

            Menu menu = new Menu("ID_Importation", "Importation", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.ItemSelectCallbackAsync = ImportationMenuCallback;
            menu.IndexChangeCallbackAsync = ItemChangeCallback;
            menu.FinalizerAsync = MenuFinalizer;

            foreach (DockItemData item in importItems)
            {
                MenuItem listItem = new MenuItem(item.Name, "", item.ItemID.ToString(), true, rightLabel: "0");
                listItem.SetInput("", 3, InputType.UNumber);
                listItem.InputSetRightLabel = true;
                listItem.SetData("DockItem", item);
                menu.Add(listItem);
            }

            menu.Add(new MenuItem("~g~Valider", "", "Validate", true));

            menu.OpenMenu(client);
            client.Emit("InitDockOrder", ((DockItemData)menu.Items[0].GetData("DockItem")).Price);
        }

        private async Task ImportationMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem.Id == "Validate")
            {
                Dictionary<DockItemData, int> importItems = new Dictionary<DockItemData, int>();

                foreach (MenuItem item in menu.Items)
                {
                    if (item.HasData("DockItem"))
                    {
                        DockItemData dockItem = item.GetData("DockItem");

                        if (int.TryParse(item.InputValue, out int quantity) && quantity > 0)
                            importItems.Add(dockItem, quantity);
                    }
                }

                bool validation = await Dock_CommandeValidate(client, menu, importItems);

                if (validation)
                    menu.CloseMenu(client);
            }
            else
                await ItemChangeCallback(client, menu, itemIndex, menuItem);
        }

        private async Task ItemChangeCallback(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            double itemPrice = 0;
            double itemTotal = 0;

            if (menuItem.HasData("DockItem"))
            {
                DockItemData dockItem = menuItem.GetData("DockItem");
                itemPrice = dockItem.Price;

                if (int.TryParse(menuItem.InputValue, out int quantity))
                    itemTotal = itemPrice * quantity;
            }

            _orderPrice = CalculateOrderTotal(menu);
            await client.EmitAsync("UpdateDockOrder", itemPrice, itemTotal, _orderPrice);
        }

        private async Task MenuFinalizer(IPlayer client, Menu menu)
        {
            await client.EmitAsync("EndDockOrder");
        }
        #endregion

        #region Private methods
        private double CalculateOrderTotal(Menu menu)
        {
            double orderTotal = 0;

            foreach (MenuItem item in menu.Items)
            {
                if (item.HasData("DockItem") && int.TryParse(item.InputValue, out int quantity))
                {
                    DockItemData dockItem = item.GetData("DockItem");
                    orderTotal += dockItem.Price * quantity;
                }
            }

            return orderTotal;
        }
        #endregion
    }
}
