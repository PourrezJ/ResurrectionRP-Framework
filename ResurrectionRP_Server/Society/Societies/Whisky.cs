using ResurrectionRP_Server.Models;
using System.Numerics;

namespace ResurrectionRP_Server.Society.Societies
{
    public class Whisky : Society
    {
        public Whisky(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
    }
}
