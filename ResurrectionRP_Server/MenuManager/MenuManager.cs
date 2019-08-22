using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class MenuManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, Menu> _clientMenus = new ConcurrentDictionary<IPlayer, Menu>();
        #endregion

        #region Constructor
        public MenuManager()
        {
            AltAsync.OnClient("MenuManager_ExecuteCallback", MenuManager_ExecuteCallbacks);
            AltAsync.OnClient("MenuManager_IndexChanged", MenuManager_IndexChanged);
            AltAsync.OnClient("MenuManager_ListChanged", MenuManager_ListChanged);
            AltAsync.OnClient("MenuManager_BackKey", MenuManager_BackKey);
            AltAsync.OnClient("MenuManager_ClosedMenu", MenuManager_ClosedMenu);
            AltAsync.OnPlayerDisconnect += OnPlayerDisconnect;
        }
        #endregion

        #region API Event handlers
        private Task OnPlayerDisconnect(ReadOnlyPlayer player, IPlayer origin, string reason)
        {
            Menu menu;
            _clientMenus.TryRemove(player, out menu);
            return Task.CompletedTask;
        }
        #endregion

        #region Private API triggers
        public async Task MenuManager_BackKey(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            Menu menu = null;
            _clientMenus.TryGetValue(player, out menu);

            if (menu != null && !menu.BackCloseMenu)
                await menu.Callback(player, menu, null, -1, null);
            else if (menu != null)
            {
                await menu.Finalizer?.Invoke(player, menu);
                _clientMenus.TryRemove(player, out menu);
            }
        }

        private async Task MenuManager_ExecuteCallbacks(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            Menu menu = null;
            MenuItem menuItem = null;

            int itemIndex = Convert.ToInt32(args[0]);

            if (itemIndex == -1)
                return;

            _clientMenus.TryGetValue(player, out menu);

            if (menu != null)
            {
                dynamic dynMenu = JsonConvert.DeserializeObject(args[1].ToString());

                for (int i = 0; i < menu.Items.Count; i++)
                {
                    var item = menu.Items[i];
                    var newitem = dynMenu.Items[i];

                    if (!string.IsNullOrEmpty((string)newitem.InputValue))
                        item.InputValue = newitem.InputValue;

                    if (item.MenuType == MenuItemType.CheckboxItem)
                        ((CheckboxItem)item).Checked = newitem.Checked;

                    if (item.MenuType == MenuItemType.ListItem)
                        ((ListItem)item).SelectedItem = newitem.SelectedItem;
                }

                if (menu.Items[itemIndex] == null)
                {
                    player.GetData("SocialClub", out string social);
                    Alt.Server.LogError($"MenuManager_ExecuteCallbacks ID: {dynMenu.Id} Player: {social}");
                    return;
                }

                if (menu.Items.Count >= itemIndex)
                {
                    try
                    {
                        menuItem = menu.Items[itemIndex];

                        if (menuItem == null)
                            return;

                        if (menu.Callback != null)
                            await menu.Callback.Invoke(player, menu, menu.Items[itemIndex], itemIndex, "");

                        if (menuItem.OnMenuItemCallback != null)
                            await menuItem.OnMenuItemCallback.Invoke(player, menu, menuItem, itemIndex, "");
                    }
                    catch(Exception ex)
                    {
                        Alt.Server.LogError(ex.ToString());
                    }
                }
            }
        }

        public async Task MenuManager_IndexChanged(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && menu.CallbackCurrentItem)
            {
                int index = Convert.ToInt32(args[0]);
                await menu.CurrentItemCallback(player, menu, index, menu.Items[index]);
            }
        }
 
        public async Task MenuManager_ListChanged(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            Menu menu = null;
            _clientMenus.TryGetValue(player, out menu);

            if (menu != null && menu.ListCallback != null)
            {
                await menu.ListCallback(player, menu, (ListItem)menu.Items[(int)args[0]], (int)args[1]);
            }
        }

        public async Task MenuManager_ClosedMenu(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            Menu menu = null;
            _clientMenus.TryGetValue(player, out menu);

            if (menu != null)
            {
                await menu.Finalizer?.Invoke(player, menu);
                _clientMenus.TryRemove(player, out menu);
            }
        }
        #endregion

        #region Public static methods
        public static async Task CloseMenu(IPlayer client)
        {
            if (_clientMenus.TryRemove(client, out Menu value))
            {
                if (value.Finalizer != null)
                 await value.Finalizer.Invoke(client, value);    
            }
            
            await client.EmitAsync("MenuManager_CloseMenu");
        }

        public async Task ForceCallback(IPlayer client)
        {
            Menu menu = null;
            _clientMenus.TryGetValue(client, out menu);

            if (menu == null || menu.Callback == null)
                return;

            await client.EmitAsync("MenuManager_ForceCallback");
        }

        public Menu GetMenu(IPlayer client)
        {
            Menu menu = null;
            _clientMenus.TryGetValue(client, out menu);

            return menu;
        }

        public static bool HasOpenMenu(IPlayer client)
        {
            return _clientMenus.ContainsKey(client);
        }

        public static async Task<bool> OpenMenu(IPlayer client, Menu menu)
        {
            if (menu.Items.Count == 0 || menu.Items == null) return false;

            Menu oldMenu = null;
            _clientMenus.TryRemove(client, out oldMenu);
            
            if (oldMenu != null)
            {
                if (oldMenu.Finalizer != null)
                    await oldMenu.Finalizer?.Invoke(client, menu);
            }

            if (_clientMenus.TryAdd(client, menu))
            {
                if (menu.CurrentItemCallback != null) menu.CallbackCurrentItem = true;
                await client.EmitAsync("MenuManager_OpenMenu", menu);
                return true;
            }

            return false;
        }
        #endregion
    }
}
