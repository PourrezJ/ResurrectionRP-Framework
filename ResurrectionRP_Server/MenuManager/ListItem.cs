using System.Collections.Generic;
using Newtonsoft.Json;

namespace ResurrectionRP_Server
{
    public class ListItem : MenuItem, IListItem
    {
        #region Public properties
        public List<object> Items { get; set; }
        public int SelectedItem { get; set; } = 0;
        public bool ExecuteCallbackListChange { get; set; }
        #endregion

        #region Constructor
        public ListItem(string text, string description, string id, int itemsMax, int selectedItem, bool executeCallback = false, bool executeCallbackListChange = false) : base(text, description, id)
        {
            MenuType = MenuItemType.ListItem;

            Items = new List<object>();

            for (int a = 0; a < itemsMax; a++)
                Items.Add(a);

            SelectedItem = selectedItem;
            ExecuteCallback = executeCallback;
            ExecuteCallbackListChange = executeCallbackListChange;
        }

        public ListItem(string text, string description, string id, List<object> items, int selectedItem, bool executeCallback = false, bool executeCallbackListChange= false) : base(text, description, id)
        {
            MenuType = MenuItemType.ListItem;
            Items = items;
            SelectedItem = selectedItem;
            ExecuteCallback = executeCallback;
            ExecuteCallbackListChange = executeCallbackListChange;
        }
        #endregion

        #region Public overrided methods
        public override bool IsInput()
        {
            return false;
        }
        #endregion
    }
}
