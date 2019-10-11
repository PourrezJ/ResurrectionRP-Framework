using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Teleport;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.XMenuManager;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class Division : Faction
    {
        private readonly Location ParkingSp1 = new Location(new Vector3(1529.647f, 6341.562f, 23.70383f), new Vector3(0.9105718f, 0.6566852f, 58.6839f));
        private readonly Location ParkingSp2 = new Location(new Vector3(1532.691f, 6329.502f, 23.84923f), new Vector3(-0.9588447f, -0.4886583f, 57.30316f));

        public Door Gate { get; private set; }

        private Teleport.Teleport teleport;

        public Division(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {

        }

        public override Faction Init()
        {
            this.ServiceLocation = new Vector3(903.7235f, -3199.718f, -97.18795f);
            this.ShopLocation = new Vector3(909.0047f, -3211.328f, -99.22219f);
            this.ParkingLocation = new Location(new Vector3(840.60657f, -3237.3625f, -98.44177f), new Vector3(0, 0, 270f));
            //this.HeliportLocation = new Location(new Vector3(1423.278f, 6374.691f, 29.10139f), new Vector3(0, 0, 258.6528f));
            this.HeliportLocation = null;

            this.FactionRang = new FactionRang[] {
                new FactionRang(0,"Recrue", false),
                new FactionRang(1,"Soldat", false),
                new FactionRang(2,"Caporal", false),
                new FactionRang(3,"Leader", true)
            };

            //ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Assault Rifle MK2", "", hash: WeaponHash.AssaultRifleMk2), 200000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Revolver MK2", "", hash: WeaponHash.RevolverMk2), 50000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Compact Rifle", "", hash: WeaponHash.CompactRifle), 100000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "SMG MK2", "", hash: WeaponHash.SmgMk2), 125000, 2));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Mini SMG", "", hash: WeaponHash.MiniSmg), 75000, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "APP Pistol", "", hash: WeaponHash.ApPistol), 75000, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Double Barrel Shotgun", "", hash: WeaponHash.DoubleBarrelShotgun), 100000, 2));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.LampeTorche, "Lampe de poche", "", hash: WeaponHash.Flashlight), 2500, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Machete, "Machete", "", hash: WeaponHash.Machete), 2500, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.BattleAxe, "Hache de combat", "", hash: WeaponHash.BattleAxe), 3500, 1));
            ItemShop.Add(new FactionShopItem(new HandCuff(ItemID.Handcuff, "Serflex", "Un serflex", 0.1, true, false), 500, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "MG", "", hash: WeaponHash.Mg), 250000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Sniper", "", hash: WeaponHash.SniperRifle), 300000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "RPG", "", hash: WeaponHash.Rpg), 400000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Horming Launcher", "", hash: WeaponHash.HomingLauncher), 500000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Grenade", "", hash: WeaponHash.Grenade), 100000, 3));
            //ItemShop.Add(new FactionShop(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(45, 0, 0), new Inventory(40, 20), 1, true, false, false, true, true, 500, classes: "backpack", icon: "backpack"), 500, 0));

            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Contender, 500000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Dukes2, 1000000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Wastlndr, 250000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Technical2, 500000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Technical3, 1000000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Barrage, 2500000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Kuruma2, 2250000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Dubsta2, 200000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Bruiser, 250000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Insurgent, 1750000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Insurgent2, 2500000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Baller6, 2000000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Schafter6, 22500000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Buzzard, 2250000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Buzzard2, 2250000));
            VehicleAllowed.Add(new FactionVehicle(2, VehicleModel.Dominator4, 1500000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Bodhi2, 75000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Imperator, 250000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Brutus, 150000, 120));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Deathbike, 180000, 5));
            VehicleAllowed.Add(new FactionVehicle(3, (VehicleModel)VehicleModel2.charge4, 180000, 90));
            
            List<TeleportEtage> etages = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Bunker", Location = new Location(new Vector3(887.0242f, -3244.5364f, -98.32385f), new Vector3(0, 0, 90))}
            };

            teleport = Teleport.Teleport.CreateTeleport(new Location(new Vector3(1015.8593f, 2906.1626f, 41.35f), new Vector3(0, 0, 348.4453f)), etages, new Vector3(3,3,0.1f), true, 60, GameMode.GlobalDimension, GameMode.GlobalDimension, "", true, FactionPlayerList.Keys.ToList(), true);

            return base.Init();
        }

        public override Task OnPlayerPromote(IPlayer client, int rang)
        {
            if (!teleport.Whileliste.Contains(client.GetSocialClub()))
                teleport.Whileliste.Add(client.GetSocialClub());

            return base.OnPlayerPromote(client, rang);
        }

        public override async Task PlayerFactionAdded(IPlayer client)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            List<ClothItem> clothItem = null;

            if (client.GetPlayerHandler()?.Character.Gender == 0)
            {
                clothItem = new List<ClothItem>()
                {
                     new ClothItem(ItemID.Mask, "", "", new ClothData(54, 0, 0), 0, false, false, false, false, false, 0, classes: "mask", icon: "mask"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(19, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(50, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.TShirt, "", "", new ClothData(15, 0, 0), 0, false, false, false, false, false, 0, classes: "shirt", icon: "shirt"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(31, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(27, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),

                     new ClothItem(ItemID.Hats, "", "", new ClothData(115, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     //new ClothItem(ItemID.Jacket, "", "", new ClothData(289, 12, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.TShirt, "", "", new ClothData(22, 0, 0), 0, false, false, false, false, false, 0, classes: "shirt", icon: "shirt"),
                };
            }
            else
            {
                clothItem = new List<ClothItem>()
                {
                     new ClothItem(ItemID.Mask, "", "", new ClothData(54, 0, 0), 0, false, false, false, false, false, 0, classes: "mask", icon: "mask"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(19, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(43, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(32, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(26, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(114, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(12, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.TShirt, "", "", new ClothData(143, 0, 0), 0, false, false, false, false, false, 0, classes: "shirt", icon: "shirt"),
                };
            }

            for (int i = 0; i < clothItem.Count; i++)
            {
                var cloth = clothItem[i];
                FactionPlayerList[client.GetSocialClub()].Inventory.AddItem(cloth, 1);
            }

            await base.PlayerFactionAdded(client);
        }

        public override XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            xmenu.SetData("Player", target);

            return base.InteractPlayerMenu(client, target, xmenu);
        }
    }
}