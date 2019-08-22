using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class MenuItem : IMenuItem
    {
        #region Private fields
        private Dictionary<string, object> _data;
        #endregion

        #region Events
        public delegate Task OnMenuItemCallBackDelegate(IPlayer client, Menu menu = null, IMenuItem menuItem = null, int itemIndex = 0, dynamic data = null);

        [JsonIgnore]
        public OnMenuItemCallBackDelegate OnMenuItemCallback { get; set; } = null;
        #endregion

        #region Public properties
        public MenuItemType? MenuType { get; set; }
        public string Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public BadgeStyle? LeftBadge { get; set; }
        public BadgeStyle? RightBadge { get; set; }
        public string RightLabel { get; set; }
        public bool ExecuteCallback { get; set; }
        public bool ExecuteCallbackIndexChange { get; set; }
        public bool ExecuteCallbackListChange { get; set; }
        public bool InputSetRightLabel { get; set; }
        public string InputValue { get; set; }
        public byte? InputMaxLength { get; set; }
        public InputType? InputType { get; set; }
        #endregion

        #region Constructor
        public MenuItem(string text, string description = null, string id = null, bool executeCallback = false, bool executeCallbackIndexChange = false, bool executeCallbackListChange = false, string rightLabel = "")
        {
            MenuType = MenuItemType.MenuItem;

            if (text != null && text.Trim().Length == 0)
                Text = null;
            else
                Text = text;

            if (description != null && description.Trim().Length == 0)
                Description = null;
            else
                Description = description;

            if (id != null && id.Trim().Length == 0)
                Id = null;
            else
                Id = id;

            LeftBadge = null;
            RightBadge = null;

            if (rightLabel != null && rightLabel.Trim().Length == 0)
                RightLabel = null;
            else
                RightLabel = rightLabel;

            ExecuteCallback = executeCallback;
            ExecuteCallbackIndexChange = executeCallbackIndexChange;
            ExecuteCallbackListChange = executeCallbackListChange;
            InputSetRightLabel = false;
            InputValue = null;
            InputMaxLength = null;
            InputType = null;
            _data = new Dictionary<string, object>();
        }
        #endregion

        #region Public virtual methods
        public virtual bool IsInput()
        {
            return InputMaxLength > 0;
        }
        #endregion

        #region Public methods
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

        public void ResetData(string key)
        {
            _data.Remove(key);
        }

        public void SetData(string key, object value)
        {
            _data[key] = value;
        }

        public void SetInput(string defaultText, byte maxLength, InputType inputType)
        {
            InputValue = defaultText;
            InputMaxLength = maxLength;
            InputType = inputType;
        }

        public ListItem CastToListItem()
        {
            return this as ListItem;
        }
        #endregion
    }
}
