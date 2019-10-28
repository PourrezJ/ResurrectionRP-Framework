using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Society.Societies
{
    public class Unicorn : Society
    {
        #region Constructor
        public Unicorn(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            List<Teleport.TeleportEtage> etages = new List<Teleport.TeleportEtage>()
            {
                new Teleport.TeleportEtage() { Name = "Sortie arrière", Location = new Location(new Vector3(132.4223f, -1287.319f, 29.27378f), new Vector3(0, 0, 28.89318f)) }
            };

            // v759 Impossible de créer un teleporteur ??? Erreur c#
            // Teleport = await Teleport.CreateTeleport(new Location(new Vector3(133.1923f, -1293.724f, 29.26952f), new Vector3(0, 0, 121.3312f)), etages, menutitle: "Porte");

            Doors = new List<Door>()
            {
                Door.CreateDoor(668467214, new Vector3(95.84587f, -1285.645f, 29.26877f), true)
                // Door.CreateDoor(-1116041313, new Vector3(128.7443f, -1298.621f, 29.23274f), true),
                // Door.CreateDoor(-626684119, new Vector3(99.5032f, -1292.784f, 29.26877f), true)
            };

            foreach (Door door in Doors)
                door.Interact = OpenDoor;

            base.Init();
        }
        #endregion
    }
}
