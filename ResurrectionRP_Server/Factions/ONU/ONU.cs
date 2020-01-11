using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Entities.Vehicles;
using AltV.Net;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Teleport;

namespace ResurrectionRP_Server.Factions
{
    public partial class ONU : Faction
    {
        #region Static fields
        private static readonly double healprice = 1000;

        #endregion


        #region Constructor
        public ONU(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }
        #endregion

        #region Init
        public override Faction Init()
        {
            ServiceLocation = new Vector3(353.1839f, -581.8728f, 43.2966f);
            ShopLocation = new Vector3(349.9324f, -580.4365f, 42.29609f);
            ParkingLocation = new Location(new Vector3(341.1377f, -559.7379f, 28.26499f), new Vector3(-0.0811837f, 0.02818833f, 339.2909f));
            HeliportLocation = new Location(new Vector3(351.8859f, -587.7157f, 74.49007f), new Vector3(0.183099f, 0.5883495f, 252.0849f));

            FactionRang = new FactionRang[] {
                    new FactionRang(0,"Soldat", false, 1500),
                    new FactionRang(1,"Caporal", false, 2000),
                    new FactionRang(2,"Sergent", false, 3000),
                    new FactionRang(3,"Lieutenant", true, 3500),
                    new FactionRang(4,"Colonel", true, 4000)
                };

            VehicleAllowed = new List<FactionVehicle>();
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Ambulance));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Insurgent2));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Insurgent3));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Polmav));
            VehicleAllowed.Add(new FactionVehicle(3, (VehicleModel)VehicleModel2.qrv));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Apc));

            BlipPosition = new Vector3(298.5557f, -584.4615f, 43.26084f);
            BlipColor = (BlipColor)57;
            BlipSprite = 61;

            ItemShop.Add(new FactionShopItem(Inventory.Inventory.ItemByID(ItemID.Bandages), 0, 0));
            ItemShop.Add(new FactionShopItem(Inventory.Inventory.ItemByID(ItemID.KitSoin), 0, 0));
            ItemShop.Add(new FactionShopItem(Inventory.Inventory.ItemByID(ItemID.Defibrilateur), 0, 0));
            ItemShop.Add(new FactionShopItem(Inventory.Inventory.ItemByID(ItemID.Donuts), 2, 0));
            ItemShop.Add(new FactionShopItem(Inventory.Inventory.ItemByID(ItemID.Coffee), 2, 0));
            ItemShop.Add(new FactionShopItem(new RadioItem(ItemID.Radio, "Talky", "", 1, true, true, false, true, true, 2000, icon: "talky"), 500, 0));
            ItemShop.Add(new FactionShopItem(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(45, 0, 0), 40, 20, 1, true, false, false, true, true, 500, classes: "backpack", icon: "backpack"), 500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Taser, "Taser", "", 3, hash: WeaponHash.StunGun), 30000, 0));
            ItemShop.Add(new FactionShopItem(new HandCuff(ItemID.Handcuff, "Menottes", "Une paire de menottes", 0.1, true, false), 500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.LampeTorche, "Lampe Torche", "Une lampe leds 500watts.", 2, hash: WeaponHash.Flashlight), 2500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Matraque, "Matraque", "Une matraque de marque Théo.", 5, hash: WeaponHash.Nightstick), 3500, 0));

            ServicePlayerList = new List<string>();


            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Pistol, "Pistol MK2", "", hash: WeaponHash.PistolMk2), 0, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Carabine, "Special Carbine MK2", "", hash: WeaponHash.SpecialCarbineMk2), 0, 3));

            List<TeleportEtage> etages = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Parking", Location = new Location(new Vector3(319.6315f, -559.7232f, 28.74379f), new Vector3(0, 0, 16.08324f))},
                //new TeleportEtage() { Name = "Intérieur", Location = new Location(new Vector3(325.285f, -598.7441f, 43.29178f), new Vector3(0, 0, 66.46763f))},
                new TeleportEtage() { Name = "Héliport", Location = new Location(new Vector3(339.0878f, -583.9037f, 74.16565f), new Vector3(0, 0, 252.1023f))}
            };

            Teleport.Teleport.CreateTeleport(new Location(new Vector3(325.285f, -598.7441f, 43.29178f), new Vector3(0, 0, 66.46763f)), etages, new Vector3(1, 1, 0.2f), menutitle: "Ascenseur");

            Entities.Peds.Ped npcmedic = Entities.Peds.Ped.CreateNPC(PedModel.Scrubs01SFY, new Vector3(308.4536f, -596.9634f, 43.29179f), 3.762666f);
            npcmedic.NpcInteractCallBack = OnNPCInteract;

            BlipsManager.CreateBlip("Clinique Médicale", new Vector3(-264.5344f, 6314.32f, 32.4364f), 57, 61, 1f);

            Entities.Peds.Ped npcmedic2 = Entities.Peds.Ped.CreateNPC(PedModel.Scrubs01SFY, new Vector3(-264.5344f, 6314.32f, 32.4364f), 320.3845f);
            npcmedic2.NpcInteractCallBack = OnNPCInteract;

            EmCall = new EmergencyCall(FactionName);
            return base.Init();
        }
        #endregion

        #region Event handlers
        public override void OnPlayerPromote(IPlayer client, int rang)
        {
            if (!client.Exists)
                return;

            var socialClub = client.GetSocialClub();

            if (client.GetPlayerHandler()?.Character.Gender == 0)
            {
                switch (rang)
                {
                    case 0:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 2, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 1:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 2:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 3:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 4:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;
                }
            }
            else
            {
                switch (FactionPlayerList[socialClub].Rang)
                {
                    case 0:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 2, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 1:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 2:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 3:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 4:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;
                }
            }

            base.OnPlayerPromote(client, rang);
        }

        public override void PlayerFactionAdded(IPlayer client)
        {
            if (!client.Exists)
                return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            var socialClub =  client.GetSocialClub();

            List<ClothItem> clothItem = null;

            if (client.GetPlayerHandler()?.Character.Gender == 0)
            {
                clothItem = new List<ClothItem>()
                {
                     new ClothItem(ItemID.Hats, "", "", new ClothData(59, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(116, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(33, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(24, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                     new ClothItem(ItemID.Mask, "", "", new ClothData(121, 0, 0), 0, false, false, false, false, false, 0, classes: "mask", icon: "mask"),
                     new ClothItem(ItemID.Necklace, "", "", new ClothData(126, 0, 0), 0, false, false, false, false, false, 0, classes: "necklace", icon: "necklace")
                };

                switch (FactionPlayerList[socialClub].Rang)
                {
                    case 0:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 2, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 1:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 2:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 3:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 4:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(106, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;
                }
            }
            else
            {
                clothItem = new List<ClothItem>()
                {
                     new ClothItem(ItemID.Hats, "", "", new ClothData(59, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(108, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(30, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(24, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                     new ClothItem(ItemID.Mask, "", "", new ClothData(121, 0, 0), 0, false, false, false, false, false, 0, classes: "mask", icon: "mask"),
                     new ClothItem(ItemID.Necklace, "", "", new ClothData(96, 0, 0), 0, false, false, false, false, false, 0, classes: "necklace", icon: "necklace")
                };

                switch (FactionPlayerList[socialClub].Rang)
                {
                    case 0:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 2, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 1:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 2:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 3:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;

                    case 4:
                        FactionPlayerList[socialClub].Inventory.AddItem(new ClothItem(ItemID.Hats, "", "", new ClothData(105, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 1);
                        break;
                }
            }

            for (int i = 0; i < clothItem.Count; i++)
            {
                var cloth = clothItem[i];
                FactionPlayerList[socialClub].Inventory.AddItem(cloth, 1);
            }

            base.PlayerFactionAdded(client);
        }

        public override void OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            if (VehicleAllowed.Exists(p => (uint)p.Hash == vehicle.Model))
            {
                if (vehicle.VehicleData.Inventory == null)
                    vehicle.VehicleData.Inventory = new Inventory.Inventory(40, 20);
            }

            base.OnVehicleOut(client, vehicle, location);
        }
        #endregion
    }
}
