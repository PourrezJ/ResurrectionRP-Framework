using System.Numerics;
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Society.Societies
{
    public class Amphitheatre : Society
    {
        #region Constructor
        public Amphitheatre(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion
    }
}
