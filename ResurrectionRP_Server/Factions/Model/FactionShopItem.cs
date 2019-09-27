using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Factions.Model
{
    public class FactionShopItem
    {
        public Item Item;
        public int Price;
        public int Rang;

        public FactionShopItem(Item item, int price, int rang)
        {
            Item = item;
            Price = price;
            Rang = rang;
        }
    }
}
