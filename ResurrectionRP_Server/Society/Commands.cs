using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Society.Societies.Bennys;

namespace ResurrectionRP_Server.Society
{
    public class Commands
    {
        public Commands()
        {
            Chat.RegisterCmd("addweazel", AddWeazel);
            Chat.RegisterCmd("addbennys", AddBennys);
        }

        private void AddBennys(IPlayer player, string[] args)
        {
            if (player.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            Parking _parking = new Parking(new Vector3(-188.0847f, -1290.69f, 31.5549f), new Location(new Vector3(-188.0847f, -1290.69f, 31.5549f), new Vector3(0.1376072f, -0.009726464f, 266.3796f)));

            var benny = new Bennys("Benny's Motorsport", player.Position, 488, 40, player.GetSocialClub(), new Inventory.Inventory(1000, 20), _parking);

            Task.Run(async ()=> await benny.Insert());
            benny.Init();
        }

        private void AddWeazel(IPlayer player, string[] args)
        {
            if (player.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            var inventory = new Inventory.Inventory(500, 40);
            var parking = new Parking(new System.Numerics.Vector3(-557.4198f, -899.7758f, 24.12378f), new Location(new System.Numerics.Vector3(-557.4198f, -899.7758f, 24.12378f), new System.Numerics.Vector3()));

            var weazel = new Weazel("Weazel News", new System.Numerics.Vector3(-580.33844f, -935.45935f, 23.871094f), 459, 1, "Armex72", inventory, parking);
            // -580.33844 -935.45935 23.871094 prise de service
            // -591.5868 -933.32306 23.871094 lifeinvader
            // -557.4198 -899.7758 24.12378 parking
            weazel.Init();
            Task.Run(async () => await weazel.Insert());
            weazel.Init();
        }


    }
}
