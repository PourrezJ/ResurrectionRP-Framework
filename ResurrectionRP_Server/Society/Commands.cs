using System;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Society.Societies;
using ResurrectionRP_Server.Society.Societies.Bennys;

namespace ResurrectionRP_Server.Society
{
    public class Commands
    {
        public Commands()
        {
            Chat.RegisterCmd("addbennys", AddBennys);
            Chat.RegisterCmd("addweazel", AddWeazel);
            Chat.RegisterCmd("addpetrol", PetrolSociety);
            Chat.RegisterCmd("addunicorn", AddUnicorn);
            Chat.RegisterCmd("addsandjob", Sandjob);
            Chat.RegisterCmd("addrhumerie", Rhumerie);
            Chat.RegisterCmd("addtequilala", TequilalaSociety);
            Chat.RegisterCmd("addyellowjack", YellowJack);
            Chat.RegisterCmd("addburgershot", BurgerShotSociety);
            Chat.RegisterCmd("addwhisky", WhiskySociety);
        }

        private void WhiskySociety(IPlayer player, string[] args)
        {
            if (player.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            Society society = new Whisky("Asgail's Brothers & Co", player.Position - new AltV.Net.Data.Position(0, 0, 1), 93, 28, "", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }

        private void BurgerShotSociety(IPlayer player, string[] args)
        {
            if (player.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            Society society = new BurgerShot("Burger Shot", player.Position - new AltV.Net.Data.Position(0, 0, 1), 106, (int)BlipColor.Orange, "", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }

        private void AddBennys(IPlayer player, string[] args)
        {
            if (player.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            Parking _parking = new Parking(new Vector3(-188.0847f, -1290.69f, 31.5549f), new Location(new Vector3(-188.0847f, -1290.69f, 31.5549f), new Vector3(0.1376072f, -0.009726464f, 266.3796f)));

            var benny = new Bennys("Benny's Motorsport", player.Position - new AltV.Net.Data.Position(0, 0, 1), 488, 40, player.GetSocialClub(), new Inventory.Inventory(1000, 20), _parking);

            Task.Run(async ()=> await benny.Insert());
            benny.Init();
        }

        private void AddWeazel(IPlayer player, string[] args)
        {
            if (player.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            var inventory = new Inventory.Inventory(500, 40);
            var parking = new Parking(new Vector3(-557.4198f, -899.7758f, 24.12378f), new Location(new System.Numerics.Vector3(-557.4198f, -899.7758f, 24.12378f), new System.Numerics.Vector3()));

            var weazel = new Weazel("Weazel News", new System.Numerics.Vector3(-580.33844f, -935.45935f, 23.871094f), 459, 1, "", inventory, parking);
            weazel.Init();
            Task.Run(async () => await weazel.Insert());
        }

        private void PetrolSociety(IPlayer client, string[] args)
        {
            if (client.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            Society society = new PetrolSociety("Au Bon Réservoir", client.Position - new AltV.Net.Data.Position(0,0,1), 351, (int)BlipColor.Grey, "", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }

        public void AddUnicorn(IPlayer client, string[] args)
        {
            if (client.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;

            var uni = new Unicorn("Vanilla Unicorn", new Vector3(132.456f, -1290.048f, 28.26952f), 121, 73, "", new Inventory.Inventory(500, 40));
            Task.Run(async () => await uni.Insert());
            uni.Init();
        }

        public void Sandjob(IPlayer client, string[] args)
        {
            if (client.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;
            Society society = new Sandjob("LosSantos Bottle", client.Position - new AltV.Net.Data.Position(0, 0, 1), 181, 28, "", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }

        public void Rhumerie(IPlayer client, string[] args)
        {
            if (client.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;
            Society society = new Rhumerie("Rhumerie de Los Santos", client.Position - new AltV.Net.Data.Position(0, 0, 1), 93, 51, "", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }

        public void TequilalaSociety(IPlayer client, string[] args)
        {
            if (client.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;
            Society society = new Tequilala("Tequi-La-La", client.Position - new AltV.Net.Data.Position(0, 0, 1), 93, (int)BlipColor.Purple, "", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }

        public void YellowJack(IPlayer client, string[] args)
        {
            if (client.GetStaffRank() < Utils.Enums.StaffRank.Moderator)
                return;
            Society society = new YellowJack("Yellow Jack", new Vector3(1982.244f, 3053.145f, 46.21507f), 93, (int)BlipColor.Yellow, "Rohakar", inventory: new Inventory.Inventory(5000, 40));
            Task.Run(async () => await society.Insert());
            society.Init();
        }
    }
}
