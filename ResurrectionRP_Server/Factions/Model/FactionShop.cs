namespace ResurrectionRP_Server.Factions.Model
{
    public class FactionShop
    {
        public Models.Item Item;
        public int Price;
        public int Rang;

        public FactionShop(Models.Item item, int price, int rang)
        {
            Item = item;
            Price = price;
            Rang = rang;
        }
    }
}
