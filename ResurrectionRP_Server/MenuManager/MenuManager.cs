﻿using AltV.Net;
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
            _clientMenus.TryRemove(player, out Menu menu);
            return Task.CompletedTask;
        }
        #endregion

        #region Private API triggers
        public async Task MenuManager_BackKey(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && !menu.BackCloseMenu)
                await menu.Callback(player, menu, null, -1);
            else if (menu != null)
            {
                await menu.Finalizer?.Invoke(player, menu);
                _clientMenus.TryRemove(player, out _);
            }
        }

        private async Task MenuManager_ExecuteCallbacks(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                string menuId = (string)args[0];
                string itemId = (string)args[1];
                int itemIndex = Convert.ToInt32(args[2]);
                bool forced = (bool)args[3];
                dynamic data = JsonConvert.DeserializeObject(args[4].ToString());

                foreach (MenuItem menuItem in menu.Items)
                {
                    if (menuItem.Type == MenuItemType.CheckboxItem)
                        ((CheckboxItem)menuItem).Checked = data[menuItem.Id];
                    else if (menuItem.Type == MenuItemType.ListItem)
                        ((ListItem)menuItem).SelectedItem = data[menuItem.Id]["Index"];
                    else if (menuItem.InputMaxLength > 0)
                        menuItem.InputValue = data[menuItem.Id];
                }

                try
                {
                    MenuItem menuItem = menu.Items[itemIndex];

                    if (menuItem == null)
                        return;

                    if (menuItem.OnMenuItemCallback != null)
                        await menuItem.OnMenuItemCallback.Invoke(player, menu, menuItem, itemIndex);

                    if (menu.Callback != null)
                        await menu.Callback.Invoke(player, menu, menu.Items[itemIndex], itemIndex);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError(ex.ToString());
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

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && menu.ListCallback != null)
                await menu.ListCallback(player, menu, (ListItem)menu.Items[(int)args[0]], (int)args[1]);
        }

        public async Task MenuManager_ClosedMenu(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                if (menu.Finalizer != null)
                    await menu.Finalizer.Invoke(player, menu);

                _clientMenus.TryRemove(player, out _);
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
            _clientMenus.TryGetValue(client, out Menu menu);

            if (menu == null || menu.Callback == null)
                return;

            await client.EmitAsync("MenuManager_ForceCallback");
        }

        public Menu GetMenu(IPlayer client)
        {
            _clientMenus.TryGetValue(client, out Menu menu);
            return menu;
        }

        public static bool HasOpenMenu(IPlayer client)
        {
            return _clientMenus.ContainsKey(client);
        }

        public static async Task<bool> OpenMenu(IPlayer client, Menu menu)
        {
            if (menu.Items.Count == 0 || menu.Items == null)
                return false;

            _clientMenus.TryRemove(client, out Menu oldMenu);
            
            if (oldMenu != null)
            {
                if (oldMenu.Finalizer != null)
                    await oldMenu.Finalizer.Invoke(client, menu);
            }

            if (_clientMenus.TryAdd(client, menu))
            {
                if (menu.CurrentItemCallback != null)
                    menu.CallbackCurrentItem = true;

                string json = JsonConvert.SerializeObject(menu, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await client.EmitAsync("MenuManager_OpenMenu", json);
                return true;
            }

            return false;
        }
        #endregion
    }
}
