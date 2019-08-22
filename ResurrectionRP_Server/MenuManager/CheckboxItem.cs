namespace ResurrectionRP_Server
{
    class CheckboxItem : MenuItem, ICheckboxItem
    {
        #region Public properties
        public bool Checked { get; set; }
        #endregion

        #region Constructor
        public CheckboxItem(string text, string description, string id, bool isChecked, bool executeCallback = false) : base(text, description, id)
        {
            MenuType = MenuItemType.CheckboxItem;
            Checked = isChecked;
            ExecuteCallback = executeCallback;
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
