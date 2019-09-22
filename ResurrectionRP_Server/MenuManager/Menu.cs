using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class Menu
    {
        #region Public enums
        public enum MenuAnchor
        {
            TopLeft = 0,
            TopCenter = 1,
            TopRight = 2,
            MiddleLeft = 3,
            MiddleCenter = 4,
            MiddleRight = 6,
            BottomLeft = 7,
            BottomCenter = 8,
            BottomRight = 9,
        }
        #endregion

        #region Public delegates
        public delegate Task MenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex);
        public delegate Task MenuCheckbox(IPlayer client, Menu menu, IMenuItem menuItem, bool value);
        public delegate Task MenuListCallback(IPlayer client, Menu menu, IListItem listItem, int listIndex);
        public delegate Task MenuCurrentIndex(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem);
        public delegate Task MenuFinalizer(IPlayer client, Menu menu);
        #endregion

        #region Private fields
        private Dictionary<string, object> _data;
        private int _selectedIndex;
        private MenuItem _selectedItem;
        #endregion

        #region Public properties
        public string Id { get; set; } = "";
        public Banner BannerSprite { get; set; }
        public MenuColor? BannerColor { get; set; }
        public string BannerTexture { get; set; }
        public string Title { get; set; } = "";
        public string SubTitle { get; set; } = "";
        public int PosX { get; set; }
        public int PosY { get; set; }
        public MenuAnchor Anchor { get; set; } = MenuAnchor.MiddleRight;
        public bool NoExit { get; set; }
        public bool EnableBanner { get; set; }
        public bool CallbackOnIndexChange
        {
            get { return IndexChangeCallback != null; }
        }
        public MenuItemList Items { get; set; }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value >= Items.Count)
                    return;

                _selectedIndex = value;
                _selectedItem = Items[(value != -1) ? value : 0];
            }
        }
        public bool BackCloseMenu { get; set; }
        #endregion

        #region Public Json ignored properties
        [JsonIgnore]
        public MenuItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (!Items.Contains(value))
                    return;

                _selectedItem = value;
                _selectedIndex = Items.IndexOf(value);
            }
        }
        [JsonIgnore]
        public MenuCallback ItemSelectCallback { get; set; }
        [JsonIgnore]
        public MenuListCallback ListItemChangeCallback { get; set; }
        [JsonIgnore]
        public MenuCurrentIndex IndexChangeCallback { get; set; }
        [JsonIgnore]
        public MenuCheckbox CallbackCheckBox { get; set; }
        [JsonIgnore]
        public MenuFinalizer Finalizer { get; set; }
        #endregion

        #region Constructor
        public Menu(string id, string title, string subTitle = "", int posX = 0, int posY = 0, MenuAnchor anchor = MenuAnchor.MiddleRight, bool noExit = false, bool enableBanner = true, bool backCloseMenu = false, Banner banner = null)
        {
            _selectedIndex = -1;
            _selectedItem = null;

            if (id == null && id.Trim().Length == 0)
                Id = null;
            else
                Id = id;

            BannerSprite = banner;
            BannerColor = new MenuColor(0, 0, 0, 0);
            BannerTexture = null;

            if (title != null && title.Trim().Length == 0)
                Title = null;
            else
                Title = title;

            if (subTitle != null && subTitle.Trim().Length > 0)
                SubTitle = subTitle;

            PosX = posX;
            PosY = posY;

            Anchor = anchor;
            NoExit = noExit;
            EnableBanner = enableBanner;
            BackCloseMenu = backCloseMenu;

            _data = new Dictionary<string, object>();
            Items = new MenuItemList();
            ItemSelectCallback = null;
            Finalizer = null;
        }
        #endregion

        #region Public operators
        public MenuItem this[string id]
        {
            get
            {
                foreach (MenuItem menuItem in Items)
                {
                    if (menuItem.Id == id)
                        return menuItem;
                }

                return null;
            }
        }
        #endregion

        #region Public methods
        public void Add(MenuItem menuItem)
        {
            Items.Add(menuItem);
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public bool Contains(string id)
        {
            foreach (MenuItem menuItem in Items)
            {
                if (menuItem.Id == id)
                    return true;
            }

            return false;
        }

        public bool Contains(MenuItem menuItem)
        {
            return Items.Contains(menuItem);
        }

        public dynamic GetData(string key)
        {
            if (!_data.ContainsKey(key))
                return null;

            return _data[key];
        }

        public bool HasData(string key)
        {
            return _data.ContainsKey(key);
        }

        public void Insert(int index, MenuItem menuItem)
        {
            Items.Insert(index, menuItem);

            if (index <= _selectedIndex)
                _selectedIndex++;
        }

        public bool Remove(string id)
        {
            foreach (MenuItem menuItem in Items)
            {
                if (menuItem.Id == id)
                    return Remove(menuItem);
            }

            return false;
        }

        public bool Remove(MenuItem menuItem)
        {
            int pos = Items.IndexOf(menuItem);
            bool result = Items.Remove(menuItem);

            if (pos == -1 || !result)
                return false;

            if (Items.Count == 0)
            {
                _selectedIndex = -1;
                _selectedItem = null;
            }
            else if (menuItem == _selectedItem)
            {
                if (_selectedIndex == Items.Count)
                    _selectedIndex--;

                _selectedItem = Items[_selectedIndex];
            }
            else if (pos < _selectedIndex)
                _selectedIndex--;

            return true;
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);

            if (Items.Count == 0)
            {
                _selectedIndex = -1;
                _selectedItem = null;
            }
            else if (index == _selectedIndex)
            {
                if (_selectedIndex == Items.Count)
                    _selectedIndex--;

                _selectedItem = Items[_selectedIndex];
            }
            else if (index < _selectedIndex)
                _selectedIndex--;
        }

        public void Reset()
        {
            Items.Clear();
            _data.Clear();
        }

        public void ResetData(string key)
        {
            _data.Remove(key);
        }

        public void SetData(string key, object value)
        {
            _data[key] = value;
        }

        public async Task<bool> OpenMenu(IPlayer client) => await MenuManager.OpenMenu(client, this);

        public async Task CloseMenu(IPlayer client) => await MenuManager.CloseMenu(client);
        #endregion
    }
}
