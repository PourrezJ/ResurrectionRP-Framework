using System.Collections.Generic;
using System.Linq;

namespace ResurrectionRP_Server
{
    public class MenuItemList : List<MenuItem>
    {
        public MenuItem this[string id]
        {
            get { return this.FirstOrDefault(i => i.Id == id); }
        }
    }
}
