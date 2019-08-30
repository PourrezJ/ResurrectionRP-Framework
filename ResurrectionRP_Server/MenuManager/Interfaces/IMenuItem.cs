namespace ResurrectionRP_Server
{
    public interface IMenuItem
    {
        string Description { get; set; }
        bool ExecuteCallback { get; set; }
        string Id { get; set; }
        byte? InputMaxLength { get; set; }
        bool InputSetRightLabel { get; set; }
        InputType? InputType { get; set; }
        string InputValue { get; set; }
        bool? InputErrorResetValue { get; set; }
        BadgeStyle? LeftBadge { get; set; }
        MenuItemType? Type { get; set; }
        MenuItem.OnMenuItemCallBackDelegate OnMenuItemCallback { get; set; }
        BadgeStyle? RightBadge { get; set; }
        string RightLabel { get; set; }
        string Text { get; set; }

        ListItem CastToListItem();
        dynamic GetData(string key);
        bool HasData(string key);
        bool IsInput();
        void ResetData(string key);
        void SetData(string key, object value);
        void SetInput(string defaultText, byte maxLength, InputType inputType, bool inputErrorResetValue);
    }
}