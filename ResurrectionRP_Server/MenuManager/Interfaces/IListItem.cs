using System.Collections.Generic;

namespace ResurrectionRP_Server
{
    public interface IListItem : IMenuItem
    {
        List<object> Items { get; set; }
        int SelectedItem { get; set; }
        bool ExecuteCallbackListChange { get; set; }
        Menu.MenuListCallbackAsync ListItemChangeCallback { get; set; }
    }
}