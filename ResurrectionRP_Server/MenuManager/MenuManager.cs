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
    public static class MenuManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, Menu> _clientMenus = new ConcurrentDictionary<IPlayer, Menu>();
        #endregion

        #region Constructor
        public static void Init()
        {
            Alt.OnClient<IPlayer, int, bool, string>("MenuManager_ExecuteCallback", MenuManager_ExecuteCallbacks);
            Alt.OnClient<IPlayer, int>("MenuManager_IndexChanged", MenuManager_IndexChanged);
            Alt.OnClient<IPlayer, int, int>("MenuManager_ListChanged", MenuManager_ListChanged);
            Alt.OnClient<IPlayer, int, int>("MenuManager_ListChanged", MenuManager_ListChanged);
            Alt.OnClient<IPlayer>("MenuManager_BackKey", MenuManager_BackKey);
            Alt.OnClient<IPlayer>("MenuManager_ClosedMenu", MenuManager_ClosedMenu);
        }

        #endregion

        #region API Event handlers
        public static void OnPlayerDisconnect(IPlayer player)
        {
            _clientMenus.TryRemove(player, out _);
        }
        #endregion

        #region Private API triggers
        public static void MenuManager_BackKey(IPlayer player)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && !menu.BackCloseMenu)
            {
                if (menu.ItemSelectCallbackAsync != null)
                    Task.Run(async () => { await menu.ItemSelectCallbackAsync(player, menu, null, -1); });

                menu.ItemSelectCallback?.Invoke(player, menu, null, -1);
            }
            else if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async () => { await menu.FinalizerAsync(player, menu); });

                menu.Finalizer?.Invoke(player, menu);
                _clientMenus.TryRemove(player, out _);
            }
        }

        private static void MenuManager_ExecuteCallbacks(IPlayer player, int itemIndex, bool forced, string datastr)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                dynamic data = JsonConvert.DeserializeObject(datastr);

                foreach (MenuItem menuItem in menu.Items)
                {
                    try
                    {
                        if (menuItem.Type == MenuItemType.CheckboxItem)
                            ((CheckboxItem)menuItem).Checked = data[menuItem.Id];
                        else if (menuItem.Type == MenuItemType.ListItem)
                            ((ListItem)menuItem).SelectedItem = data[menuItem.Id]["Index"];
                        else if (menuItem.InputMaxLength > 0)
                            menuItem.InputValue = data[menuItem.Id];
                    }
                    catch (Exception)
                    { }
                }

                try
                {
                    if (itemIndex >= menu.Items.Count)
                        return;

                    MenuItem menuItem = menu.Items[itemIndex];

                    if (menuItem == null)
                        return;

                    if (menu.ItemSelectCallbackAsync != null)
                        Task.Run(async () => { await menu.ItemSelectCallbackAsync(player, menu, menuItem, itemIndex); });

                    menu.ItemSelectCallback?.Invoke(player, menu, menuItem, itemIndex);

                    if (menuItem.OnMenuItemCallbackAsync != null)
                        Task.Run(async () => { await menuItem.OnMenuItemCallbackAsync(player, menu, menuItem, itemIndex); });

                    menuItem.OnMenuItemCallback?.Invoke(player, menu, menuItem, itemIndex);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError(ex.ToString());
                }
            }
        }

        public static void MenuManager_IndexChanged(IPlayer player, int index)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                if (menu.IndexChangeCallbackAsync != null)
                    Task.Run(async () => { await menu.IndexChangeCallbackAsync(player, menu, index, menu.Items[index]); });

                menu.IndexChangeCallback?.Invoke(player, menu, index, menu.Items[index]);
            }
        }

        public static void MenuManager_ListChanged(IPlayer player, int unk1, int unk2)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                if (menu.ListItemChangeCallbackAsync != null)
                    Task.Run(async () => { await menu.ListItemChangeCallbackAsync(player, menu, (ListItem)menu.Items[unk1], unk2); });

                menu.ListItemChangeCallback?.Invoke(player, menu, (ListItem)menu.Items[unk1], unk2);
            }
        }

        public static void MenuManager_ClosedMenu(IPlayer player)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async () => await menu.FinalizerAsync(player, menu));

                menu.Finalizer?.Invoke(player, menu);
                _clientMenus.TryRemove(player, out _);
            }
        }
        #endregion

        #region Public static methods
        public static void CloseMenu(IPlayer client)
        {
            if (_clientMenus.TryRemove(client, out Menu menu) && menu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async () => { await menu.FinalizerAsync(client, menu); });

                menu.Finalizer?.Invoke(client, menu);
            }

            client.EmitLocked("MenuManager_CloseMenu");
        }

        public static void ForceCallback(IPlayer client)
        {
            _clientMenus.TryGetValue(client, out Menu menu);

            if (menu != null && (menu.ItemSelectCallbackAsync != null || menu.ItemSelectCallback != null))
                client.EmitLocked("MenuManager_ForceCallback");
        }

        public static Menu GetMenu(IPlayer client)
        {
            _clientMenus.TryGetValue(client, out Menu menu);
            return menu;
        }

        public static bool HasOpenMenu(IPlayer client)
        {
            return _clientMenus.ContainsKey(client);
        }

        public static bool OpenMenu(IPlayer client, Menu menu)
        {
            if (menu.Items.Count == 0 || menu.Items == null)
                return false;

            _clientMenus.TryRemove(client, out Menu oldMenu);

            if (oldMenu != null)
            {
                if (oldMenu.FinalizerAsync != null)
                    Task.Run(async () => { await oldMenu.FinalizerAsync(client, menu); });

                oldMenu.Finalizer?.Invoke(client, menu);
            }

            if (_clientMenus.TryAdd(client, menu))
            {
                string json = JsonConvert.SerializeObject(menu, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                client.EmitLocked("MenuManager_OpenMenu", json);
                return true;
            }

            return false;
        }
        #endregion
    }
}