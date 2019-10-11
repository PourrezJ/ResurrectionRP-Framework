using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Society
{
    public class Commands
    {
        public Commands()
        {
            Chat.RegisterCmd("addweazel", AddWeazel);
        }

        private async Task AddWeazel(IPlayer player, string[] args)
        {
            var inventory = new Inventory.Inventory(500, 40);
            var parking = new Parking(new System.Numerics.Vector3(-557.4198f, -899.7758f, 24.12378f), new Location(new System.Numerics.Vector3(-557.4198f, -899.7758f, 24.12378f), new System.Numerics.Vector3()));

            var weazel = new Weazel("Weazel News", new System.Numerics.Vector3(-580.33844f, -935.45935f, 23.871094f), 459, 1, "Armex72", inventory, parking);
            // -580.33844 -935.45935 23.871094 prise de service
            // -591.5868 -933.32306 23.871094 lifeinvader
            // -557.4198 -899.7758 24.12378 parking
            weazel.Init();
            await weazel.Insert();   
        }
    }
}
