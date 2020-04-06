using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System.Collections.Concurrent;

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
            Alt.OnClient<IPlayer, int, string>("XMenuManager_ExecuteCallback", XMenuManager_ExecuteCallback);
            Alt.OnClient<IPlayer>("XMenuManager_ClosedMenu", XMenuManager_ClosedMenu);
        }
        #endregion

        #region Sync Callback
        private static void XMenuManager_ExecuteCallback(IPlayer client, int menuIndex, string data)
        {
            if (!client.Exists)
                return;

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
                    menu.Callback?.Invoke(client, menu, menu.Items[menuIndex], menuIndex, "");
                }
            }
        }

        public static void XMenuManager_ClosedMenu(IPlayer client)
        {
            if (!client.Exists)
                return;

            _clientMenus.TryGetValue(client, out XMenu menu);
            if (menu != null)
            {
                menu.Finalizer?.Invoke(client, menu);
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
                Alt.Server.LogInfo($"Warning {menu.Id} have more 8 items");
            _clientMenus.TryRemove(client, out XMenu oldMenu);

            if (oldMenu != null)
            {
                menu.Finalizer?.Invoke(client, menu);
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
                menu.Finalizer?.Invoke(client, menu);
                client.EmitLocked("XMenuManager_CloseMenu");
            }
        }

        internal static bool HasOpenMenu(IPlayer client) => _clientMenus.ContainsKey(client);
        #endregion
    }
}
