namespace ResurrectionRP_Server
{
    class ColoredItem : MenuItem, IColoredItem
    {
        #region Public properties
        public string BackgroundColor { get; }
        public string HighlightColor { get; }
        #endregion

        #region Constructor
        public ColoredItem(string text, string description, string id, string backgroundColor, string highlightColor) : base(text, description, id)
        {
            MenuType = MenuItemType.ColoredItem;
            BackgroundColor = backgroundColor;
            HighlightColor = highlightColor;
        }
        #endregion
    }
}
