using System;

namespace ResurrectionRP_Server.Factions.Model
{
    public class FactionPlayer
    {
        public string SocialClub;
        public int Rang;
        public DateTime LastPayCheck = DateTime.Now;
        public Inventory.Inventory Inventory;

        public FactionPlayer(string social, int rang)
        {
            SocialClub = social;
            Rang = rang;
            Inventory = new Inventory.Inventory(100, 20);
        }
    }
}
