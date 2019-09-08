using System.Numerics;
namespace ResurrectionRP_Server.Society.Societies
{
    public class Sandjob : Society
    {
        public Sandjob(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
    }
}
