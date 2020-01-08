using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public static class MenuManager
    {
        #region Private static properties
        private static Dictionary<IPlayer, Menu> _clientMenus = new Dictionary<IPlayer, Menu>();
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
            _clientMenus.Remove(player, out _);
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
                menu.ItemSelectCallback?.Invoke(player, menu, null, -1);
            }
            else if (menu != null)
            {
                menu.Finalizer?.Invoke(player, menu);
                _clientMenus.Remove(player, out _);
            }
        }

        private static void MenuManager_ExecuteCallbacks(IPlayer player, int itemIndex, bool forced, string datastr)
        {
            if (!player.Exists)
                return;

            if (_clientMenus.TryGetValue(player, out Menu menu))
            {
                dynamic data = JsonConvert.DeserializeObject(datastr);


                for (int i = 0; i < menu.Items.Count; i++)
                {
                    MenuItem menuItem = menu.Items[i];

                    if (menuItem == null)
                        continue;

                    if (string.IsNullOrEmpty(menuItem.Id))
                        continue;

                    if (menuItem.Type == MenuItemType.CheckboxItem)
                        ((CheckboxItem)menuItem).Checked = data[i.ToString()];
                    else if (menuItem.Type == MenuItemType.ListItem)
                        ((ListItem)menuItem).SelectedItem = data[i.ToString()];
                    else if (menuItem.InputMaxLength > 0)
                        menuItem.InputValue = data[i.ToString()];
                }

                if (itemIndex >= menu.Items.Count)
                    return;

                MenuItem item = menu.Items[itemIndex];

                if (item == null)
                    return;

                //if (menu.ItemSelectCallbackAsync != null)
                //    Task.Run(async () => { await menu.ItemSelectCallbackAsync(player, menu, menuItem, itemIndex); });

                menu.ItemSelectCallback?.Invoke(player, menu, item, itemIndex);
                item.OnMenuItemCallback?.Invoke(player, menu, item, itemIndex);
            }
        }

        public static void MenuManager_IndexChanged(IPlayer player, int index)
        {
            if (!player.Exists)
                return;

            if (_clientMenus.TryGetValue(player, out Menu menu))
            {
                menu.IndexChangeCallback?.Invoke(player, menu, index, menu.Items[index]);
            }
        }

        public static void MenuManager_ListChanged(IPlayer player, int unk1, int unk2)
        {
            if (!player.Exists)
                return;

            if (_clientMenus.TryGetValue(player, out Menu menu))
            {
                menu.ListItemChangeCallback?.Invoke(player, menu, (ListItem)menu.Items[unk1], unk2);
            }
        }

        public static void MenuManager_ClosedMenu(IPlayer player)
        {
            if (!player.Exists)
                return;

            if (_clientMenus.TryGetValue(player, out Menu menu))
            {
                menu.Finalizer?.Invoke(player, menu);
                _clientMenus.Remove(player, out _);
            }
        }
        #endregion

        #region Public static methods
        public static void CloseMenu(IPlayer client)
        {
            if (_clientMenus.Remove(client, out Menu menu))
            {
                menu.Finalizer?.Invoke(client, menu);
            }

            client.EmitLocked("MenuManager_CloseMenu");
        }

        public static void ForceCallback(IPlayer client)
        {
            if (_clientMenus.TryGetValue(client, out Menu menu) && menu.ItemSelectCallback != null)
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

            if (_clientMenus.Remove(client, out Menu oldMenu))
            {
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