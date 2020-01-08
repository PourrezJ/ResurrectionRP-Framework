using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ResurrectionRP_Server.XMenuManager
{
    public class XMenu
    {
        #region Public delegates
        public delegate void XMenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data);
        public delegate void XMenuFinalizer(IPlayer client, XMenu menu);
        #endregion

        #region Private fields
        private Dictionary<string, object> _data;
        private List<XMenuItem> _items;
        private int _selectedIndex;
        private XMenuItem _selectedItem;
        #endregion

        #region Public properties
        public string Id { get; set; }

        [JsonIgnore]
        public XMenuCallback Callback { get; set; }
        [JsonIgnore]
        public XMenuFinalizer Finalizer { get; set; }

        public List<XMenuItem> Items
        {
            get => _items;
            set
            {
                _items = value;
            }
        }
        #endregion

        #region Constructor
        public XMenu(string Id)
        {
            this.Id = Id;
            _data = new Dictionary<string, object>();
            _items = new List<XMenuItem>();

            //Callback = null;
        }
        #endregion

        #region Public operators
        public XMenuItem this[string id]
        {
            get
            {
                foreach (XMenuItem menuItem in _items)
                {
                    if (menuItem.Id == id)
                        return menuItem;
                }

                return null;
            }
        }
        #endregion

        #region Public methods

        public void OpenXMenu(IPlayer client)
        {
            XMenuManager.OpenMenu(client, this);
        }


        public void Add(XMenuItem menuItem)
        {
            _items.Add(menuItem);
        }

        public void ClearItems()
        {
            _items.Clear();
        }

        public bool Contains(string id)
        {
            foreach (XMenuItem menuItem in _items)
            {
                if (menuItem.Id == id)
                    return true;
            }

            return false;
        }

        public bool Contains(XMenuItem menuItem)
        {
            return _items.Contains(menuItem);
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

        public void Insert(int index, XMenuItem menuItem)
        {
            _items.Insert(index, menuItem);

            if (index <= _selectedIndex)
                _selectedIndex++;
        }

        public bool Remove(string id)
        {
            foreach (XMenuItem menuItem in _items)
            {
                if (menuItem.Id == id)
                    return Remove(menuItem);
            }

            return false;
        }

        public bool Remove(XMenuItem menuItem)
        {
            int pos = _items.IndexOf(menuItem);
            bool result = _items.Remove(menuItem);

            if (pos == -1 || !result)
                return false;

            if (_items.Count == 0)
            {
                _selectedIndex = -1;
                _selectedItem = null;
            }
            else if (menuItem == _selectedItem)
            {
                if (_selectedIndex == _items.Count)
                    _selectedIndex--;

                _selectedItem = _items[_selectedIndex];
            }
            else if (pos < _selectedIndex)
                _selectedIndex--;

            return true;
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);

            if (_items.Count == 0)
            {
                _selectedIndex = -1;
                _selectedItem = null;
            }
            else if (index == _selectedIndex)
            {
                if (_selectedIndex == _items.Count)
                    _selectedIndex--;

                _selectedItem = _items[_selectedIndex];
            }
            else if (index < _selectedIndex)
                _selectedIndex--;
        }

        public void Reset()
        {
            _items.Clear();
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

        #endregion

    }
}
