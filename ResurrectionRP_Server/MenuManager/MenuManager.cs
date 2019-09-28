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
            //AltAsync.OnClient("MenuManager_ExecuteCallback", MenuManager_ExecuteCallbacks);
            //AltAsync.OnClient("MenuManager_IndexChanged", MenuManager_IndexChanged);
            //AltAsync.OnClient("MenuManager_ListChanged", MenuManager_ListChanged);
            //AltAsync.OnClient("MenuManager_BackKey", MenuManager_BackKey);
            //AltAsync.OnClient("MenuManager_ClosedMenu", MenuManager_ClosedMenu);

            Alt.OnClient("MenuManager_ExecuteCallback", MenuManager_ExecuteCallbacks);
            Alt.OnClient("MenuManager_IndexChanged", MenuManager_IndexChanged);
            Alt.OnClient("MenuManager_ListChanged", MenuManager_ListChanged);
            Alt.OnClient("MenuManager_BackKey", MenuManager_BackKey);
            Alt.OnClient("MenuManager_ClosedMenu", MenuManager_ClosedMenu);
        }

        #endregion

        #region API Event handlers
        public static void OnPlayerDisconnect(IPlayer player)
        {
            _clientMenus.TryRemove(player, out _);
        }
        #endregion

        #region Private API triggers
        public void MenuManager_BackKey(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && !menu.BackCloseMenu && menu.ItemSelectCallbackAsync != null)
                Task.Run(async ()=>await menu.ItemSelectCallbackAsync(player, menu, null, -1));
            else if (menu != null && !menu.BackCloseMenu && menu.ItemSelectCallback != null)
                menu.ItemSelectCallback(player, menu, null, -1);


            else if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async ()=> await menu.FinalizerAsync(player, menu));
                else if (menu.Finalizer != null)
                    menu.Finalizer(player, menu);

                _clientMenus.TryRemove(player, out _);
            }
        }

        private void MenuManager_ExecuteCallbacks(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                int itemIndex = Convert.ToInt32(args[0]);
                bool forced = (bool)args[1];
                dynamic data = JsonConvert.DeserializeObject(args[2].ToString());

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
                        Task.Run(async ()=> await menu.ItemSelectCallbackAsync(player, menu, menuItem, itemIndex));
                    else if (menu.ItemSelectCallback != null)
                        menu.ItemSelectCallback(player, menu, menuItem, itemIndex);

                    if (menuItem.OnMenuItemCallbackAsync != null)
                        Task.Run(async ()=> await menuItem.OnMenuItemCallbackAsync(player, menu, menuItem, itemIndex));
                    else if (menuItem.OnMenuItemCallback != null)
                        menuItem.OnMenuItemCallback(player, menu, menuItem, itemIndex);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError(ex.ToString());
                }
            }
        }

        public void MenuManager_IndexChanged(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && menu.IndexChangeCallbackAsync != null)
            {
                int index = Convert.ToInt32(args[0]);
                Task.Run(async ()=> await menu.IndexChangeCallbackAsync(player, menu, index, menu.Items[index]));
            }
        }

        public void MenuManager_ListChanged(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null && menu.ListItemChangeCallbackAsync != null)
                Task.Run(async ()=> await menu.ListItemChangeCallbackAsync(player, menu, (ListItem)menu.Items[Convert.ToInt32(args[0])], Convert.ToInt32(args[1])));
        }

        public void MenuManager_ClosedMenu(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            _clientMenus.TryGetValue(player, out Menu menu);

            if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async ()=> await menu.FinalizerAsync(player, menu));

                _clientMenus.TryRemove(player, out _);
            }
        }
        #endregion

        #region Public static methods
        public static void CloseMenu(IPlayer client)
        {
            if (_clientMenus.TryRemove(client, out Menu menu) && menu != null && menu.FinalizerAsync != null)
                Task.Run(async ()=> await menu.FinalizerAsync(client, menu));

            client.EmitLocked("MenuManager_CloseMenu");
        }

        public void ForceCallback(IPlayer client)
        {
            _clientMenus.TryGetValue(client, out Menu menu);

            if (menu != null && menu.ItemSelectCallbackAsync != null)
                client.EmitLocked("MenuManager_ForceCallback");
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

        public static bool OpenMenu(IPlayer client, Menu menu)
        {
            if (menu.Items.Count == 0 || menu.Items == null)
                return false;

            _clientMenus.TryRemove(client, out Menu oldMenu);

            if (oldMenu != null && oldMenu.FinalizerAsync != null)
                Task.Run(async ()=> await oldMenu.FinalizerAsync(client, menu));

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