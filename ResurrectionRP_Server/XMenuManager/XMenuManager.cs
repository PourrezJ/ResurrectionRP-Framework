using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.XMenuManager
{
    public static class XMenuManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, XMenu> _clientMenus = new ConcurrentDictionary<IPlayer, XMenu>();
        #endregion

        #region Constructor
        public static void Init()
        {
            Alt.OnClient("XMenuManager_ExecuteCallback", XMenuManager_ExecuteCallback);
            Alt.OnClient("XMenuManager_ClosedMenu", XMenuManager_ClosedMenu);
        }
        #endregion

        #region Sync Callback
        private static void XMenuManager_ExecuteCallback(IPlayer client, object[] args)
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
                            menu.Items[menuIndex].InputValue = temp.Items[menuIndex].InputValue;
                    }

                    if (menu.Items[menuIndex] != null)
                    {
                        menu.Items[menuIndex].OnMenuItemCallback?.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");

                        if (menu.Items[menuIndex].OnMenuItemCallbackAsync != null)
                            Task.Run(async ()=> await menu.Items[menuIndex].OnMenuItemCallbackAsync.Invoke(client, menu, menu.Items[menuIndex], menuIndex, ""));

                        menu.Callback?.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");

                        if (menu.CallbackAsync != null)
                            Task.Run(async ()=> await menu.CallbackAsync.Invoke(client, menu, menu.Items[menuIndex], menuIndex, ""));
                    }
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        public static void XMenuManager_ClosedMenu(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            _clientMenus.TryGetValue(client, out XMenu menu);
            if (menu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async ()=> await menu.FinalizerAsync.Invoke(client, menu));
                else if (menu.Finalizer != null)
                    menu.Finalizer.Invoke(client, menu);


                client.EmitLocked("XMenuManager_CloseMenu");
            }
            else if (menu != null)
                _clientMenus.TryRemove(client, out menu);
        }
        #endregion

        #region Public static methods
        public static bool OpenMenu(IPlayer client, XMenu menu)
        {
            if (menu.Items.Count == 0 || menu.Items == null) return false;
            if (menu.Items.Count > 8)
                Alt.Server.LogInfo($"ATTENTION LE XMENU {menu.Id} contient plus de 8 items");
            _clientMenus.TryRemove(client, out XMenu oldMenu);

            if (oldMenu != null)
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async () => await menu.FinalizerAsync.Invoke(client, menu));
                else if (menu.Finalizer != null)
                    menu.Finalizer.Invoke(client, menu);

                client.EmitLocked("XMenuManager_CloseMenu");
            }

            if (_clientMenus.TryAdd(client, menu))
            {
                client.EmitLocked("XMenuManager_OpenMenu", JsonConvert.SerializeObject(menu));
                return true;
            }
            return false;
        }

        public static void CloseMenu(IPlayer client)
        {
            if (_clientMenus.TryRemove(client, out XMenu menu))
            {
                if (menu.FinalizerAsync != null)
                    Task.Run(async () => await menu.FinalizerAsync.Invoke(client, menu));
                else if (menu.Finalizer != null)
                    menu.Finalizer.Invoke(client, menu);
                client.EmitLocked("XMenuManager_CloseMenu");
            }
        }

        internal static bool HasOpenMenu(IPlayer client) => _clientMenus.ContainsKey(client);
        #endregion
    }
}
