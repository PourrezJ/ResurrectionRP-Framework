using System.Collections.Generic;

namespace ResurrectionRP_Server.Models
{
    public class ItemStack
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }

        public Dictionary<string, dynamic> Variables = new Dictionary<string, dynamic>();

        public ItemStack(Item item, int quantity, int price = 0)
        {
            this.Item = item;
            this.Quantity = quantity;
            this.Price = price;
        }

        public void SetData(string key, object value) => Variables.Add(key, value);
        public dynamic GetData(string key) => Variables.GetValueOrDefault(key);
        public void ResetData(string key) => Variables[key] = null;
        public bool HasData(string key) => Variables.ContainsKey(key);

        internal object Clone()
        {
            return MemberwiseClone();
        }
    }
}
