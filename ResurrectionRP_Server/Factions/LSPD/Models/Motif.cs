using System;
namespace ResurrectionRP_Server.Factions
{
    public class Motif
    {
        public String name;
        public int price;
        public String desc;
        public Motif(String name, int price, String desc = null)
        {
            this.name = name;
            this.price = price;
            if (desc != null)
                this.desc = desc;
        }
    }
}
