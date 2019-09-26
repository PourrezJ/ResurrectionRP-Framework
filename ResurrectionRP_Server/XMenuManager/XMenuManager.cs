using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.XMenuManager
{
    public class XMenuManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, XMenu> _clientMenus = new ConcurrentDictionary<IPlayer, XMenu>();
        #endregion

        #region Constructor
        public XMenuManager()
        {
            AltAsync.OnClient("XMenuManager_ExecuteCallback", XMenuManager_ExecuteCallbackAsync);
            AltAsync.OnClient("XMenuManager_ClosedMenu", XMenuManager_ClosedMenuAsync);

            Alt.OnClient("XMenuManager_ExecuteCallback", XMenuManager_ExecuteCallback);
            Alt.OnClient("XMenuManager_ClosedMenu", XMenuManager_ClosedMenu);
        }
        #endregion

        #region Async CallBack
        private async Task XMenuManager_ExecuteCallbackAsync(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            try
            {
                int menuIndex = Convert.ToInt32(args[0]);
                string data = args[1].ToString();

                if (_clientMenus.TryGetValue(client, out XMenu menu))
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        XMenu temp = JsonConvert.DeserializeObject<XMenu>(data);

                        if (!string.IsNullOrEmpty(temp.Items[menuIndex]?.InputValue))
                        {
                            menu.Items[menuIndex].InputValue = temp.Items[menuIndex].InputValue;
                        }
                    }

                    if (menu.Items[menuIndex] != null)
                    {
                        if (menu.Items[menuIndex].OnMenuItemCallback != null)
                            await menu.Items[menuIndex].OnMenuItemCallback.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");

                        if (menu.CallbackAsync != null)
                            await menu.CallbackAsync.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");
                    }
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        public async Task XMenuManager_ClosedMenuAsync(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            _clientMenus.TryGetValue(client, out XMenu menu);
            if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    await menu.FinalizerAsync.Invoke(client, menu);
                client.EmitLocked("XMenuManager_CloseMenu");
            }
            else if (menu != null)
                _clientMenus.TryRemove(client, out menu);
        }
        #endregion

        #region Sync Callback
        private void XMenuManager_ExecuteCallback(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            try
            {
                int menuIndex = Convert.ToInt32(args[0]);
                string data = args[1].ToString();

                if (_clientMenus.TryGetValue(client, out XMenu menu))
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        XMenu temp = JsonConvert.DeserializeObject<XMenu>(data);

                        if (!string.IsNullOrEmpty(temp.Items[menuIndex]?.InputValue))
                        {
                            menu.Items[menuIndex].InputValue = temp.Items[menuIndex].InputValue;
                        }
                    }

                    if (menu.Items[menuIndex] != null)
                    {
                        if (menu.Items[menuIndex].OnMenuItemCallback != null)
                            menu.Items[menuIndex].OnMenuItemCallback.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");

                        if (menu.CallbackAsync != null)
                            menu.CallbackAsync.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");
                    }
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        public void XMenuManager_ClosedMenu(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            _clientMenus.TryGetValue(client, out XMenu menu);
            if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    menu.FinalizerAsync.Invoke(client, menu);
                client.EmitLocked("XMenuManager_CloseMenu");
            }
            else if (menu != null)
                _clientMenus.TryRemove(client, out menu);
        }
        #endregion

        #region Public static methods
        public static async Task<bool> OpenMenu(IPlayer client, XMenu menu)
        {
            if (menu.Items.Count == 0 || menu.Items == null) return false;
            if (menu.Items.Count > 8)
                Alt.Server.LogInfo($"ATTENTION LE XMENU {menu.Id} contient plus de 8 items");
            _clientMenus.TryRemove(client, out XMenu oldMenu);

            if (oldMenu != null)
            {
                if (oldMenu.FinalizerAsync != null)
                    await oldMenu.FinalizerAsync.Invoke(client, menu);
                client.EmitLocked("XMenuManager_CloseMenu");
                await Task.Delay(100);
            }

            if (_clientMenus.TryAdd(client, menu))
            {
                client.EmitLocked("XMenuManager_OpenMenu", JsonConvert.SerializeObject(menu));
                return true;
            }
            return false;
        }

        public static async Task CloseMenu(IPlayer client)
        {
            if (_clientMenus.TryRemove(client, out XMenu menu))
            {
                if (menu.FinalizerAsync != null)
                    await menu.FinalizerAsync.Invoke(client, menu);
                client.EmitLocked("XMenuManager_CloseMenu");
            }
        }
        #endregion
    }
}
