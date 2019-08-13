using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Models.Inventory
{
    public class ItemStack
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }

        public Dictionary<dynamic, dynamic> Variables = new Dictionary<dynamic, dynamic>();

        public ItemStack(Item item, int quantity, int price = 0)
        {
            Item = item;
            Quantity = quantity;
            Price = price;
        }

        public void SetData(string key, object value) => Variables.Add(key, value);
        public dynamic GetData(string key) => Variables.GetValueOrDefault(key);
        public void ResetData(string key) => Variables[key] = null;
        public bool HasData(string key) => Variables.ContainsKey(key);

        internal object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
