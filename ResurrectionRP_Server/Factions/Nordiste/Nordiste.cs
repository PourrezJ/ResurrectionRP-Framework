using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Phone;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public partial class Nordiste : Faction
    {
        #region Private fields
        private Ped _pedArmurerie;
        private Ped _pedAccueil;
        #endregion

        #region Fields and properties
        public List<Invoice> InvoiceList = new List<Invoice>();

        public List<Door> Doors { get; private set; }
        #endregion

        #region Constructor
        public Nordiste(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }
        #endregion

        #region Init
        public override Faction Init()
        {
            FactionRang = new FactionRang[] {
                new FactionRang(0,"Aspirant", false, 1500),
                new FactionRang(1,"Officier ", false, 2000),
                new FactionRang(2,"Lieutenant ", false, 2500),
                new FactionRang(3,"Sheriff Adjoint ", false, 3000),
                new FactionRang(4,"Sheriff ", true, 3500, true),
                new FactionRang(5,"Marshall ", true, 4000, true, true),
                new FactionRang(6,"Maire ", true, 4500, true, true)
            };

            ServiceLocation = new Vector3(-454.7729f, 6015.708f, 31.71646f);
            ParkingLocation = new Location(new Vector3(-468.8465f, 6029.152f, 31.34055f), new Vector3(0, 0, 208.0693f));
            //ShopLocation = new Vector3(-436.0355f, 5999.875f, 31.71611f);

            BlipPosition = ServiceLocation;
            BlipColor = (BlipColor)5;
            BlipSprite = 487;

            ItemShop.Add(new FactionShopItem(new HandCuff(ItemID.Handcuff, "Menottes", "Une paire de menottes", 0.1, true, false), 500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.LampeTorche, "Lampe Torche", "Une lampe leds 500watts.", 2, hash: WeaponHash.Flashlight), 2500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Matraque, "Matraque", "Une matraque de marque Théo.", 5, hash: WeaponHash.Nightstick), 3500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Pistolet MK2", "", 3, hash: WeaponHash.PistolMk2), 40000, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Taser, "Taser", "", 3, hash: WeaponHash.StunGun), 30000, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Flare Gun", "", 5, hash: WeaponHash.FlareGun), 20000, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Revolver MK2", "", 4, hash: WeaponHash.RevolverMk2), 50000, 2));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Assault SMG", "", 10, hash: WeaponHash.AssaultSmg), 125000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Combat MG", "", 26, hash: WeaponHash.CombatMg), 350000, 3));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Combat MG MK2", "", 26, hash: WeaponHash.CombatMgMk2), 400000, 3));
            ItemShop.Add(new FactionShopItem(new RadioItem(ItemID.Radio, "Talky", "", 1, true, true, false, true, true, 2000, icon: "talky"), 500, 0));
            ItemShop.Add(new FactionShopItem(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(45, 0, 0), 40, 20, 1, true, false, false, true, true, 500, classes: "backpack", icon: "backpack"), 500, 0));

            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sheriff, 150000, 40, 20, 132, 0));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Policeb, 150000, 15, 10, 132, 0));

            #region Peds
            _pedArmurerie = Ped.CreateNPC(PedModel.Sheriff01SMY, new Vector3(-436.0355f, 5999.875f, 31.71611f), 52.54101f);
            _pedArmurerie.NpcInteractCallBack = OnNPCInteract;

            _pedAccueil = Ped.CreateNPC(PedModel.Sheriff01SFY, new Vector3(-448.7506f, 6012.733f, 31.71646f), 315.8641f);
            _pedAccueil.NpcInteractCallBack = OnNPCInteract;
            #endregion

            #region Doors

            // porte devant
            var a = Door.CreateDoor(2793810241, new Vector3(-444.0901f, 6016.447f, 31.71648f), false);
            var b = Door.CreateDoor(2793810241, new Vector3(-443.2377f, 6015.598f, 31.71648f), false);

            // porte arrière
            var c = Door.CreateDoor(2271212864, new Vector3(-451.387f, 6006.599f, 31.83825f), false);
            var d = Door.CreateDoor(2271212864, new Vector3(-446.7517f, 6001.856f, 31.71646f), false);

            // armurerie
            var e = Door.CreateDoor(749848321, new Vector3(-438.782f, 5998.675f, 31.71612f), false);

            // interogatoire
            var f = Door.CreateDoor(749848321, new Vector3(-437.2475f, 6002.089f, 27.98565f), false);

            // cellule
            var g = Door.CreateDoor(631614199, new Vector3(-444.7756f, 6011.588f, 27.98565f), false);
            var h = Door.CreateDoor(631614199, new Vector3(-442.7826f, 6009.607f, 27.98567f), false);
            var i = Door.CreateDoor(631614199, new Vector3(-438.8031f, 6005.836f, 27.98566f), false);

            Doors = new List<Door>() { a, b, c, d, e, f, g, h, i };

            foreach (var door in Doors)
                door.Interact = OpenCelluleDoor;
            #endregion

            if (InvoiceList == null)
                InvoiceList = new List<Invoice>();

            Alt.OnClient("CallUrgenceLSPD", CallUrgenceLSPD);

            return base.Init();
        }
        #endregion

        #region Event handlers
        private void OnNPCInteract(IPlayer client, Ped npc)
        {
            if (npc == _pedArmurerie)
                OpenShopMenu(client);
            else if (npc == _pedAccueil)
                OpenAccueilMenu(client);
        }

        public override void OnPlayerPromote(IPlayer client, int rang)
        {
            if (!client.Exists)
                return;

            base.OnPlayerPromote(client, rang);
        }

        public override void PlayerFactionAdded(IPlayer client)
        {
            if (!client.Exists)
                return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            List<ClothItem> clothItem = null;

            if (client.GetPlayerHandler()?.Character.Gender == 0)
            {
                clothItem = new List<ClothItem>()
                {
                     new ClothItem(ItemID.Hats, "", "", new ClothData(108, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(105, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(165, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(75, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(27, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                };
            }
            else
            {
                clothItem = new List<ClothItem>()
                {
                     new ClothItem(ItemID.Hats, "", "", new ClothData(107, 1, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(104, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(179, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(73, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(26, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                };
            }

            var socialClub = client.GetSocialClub();

            for (int i = 0; i < clothItem.Count; i++)
            {
                var cloth = clothItem[i];
                FactionPlayerList[socialClub].Inventory.AddItem(cloth, 1);
            }

            base.PlayerFactionAdded(client);
        }

        public override Task OnVehicleOut(IPlayer client, VehicleHandler vehicleHandler, Location location)
        {
            if (VehicleAllowed.Exists(p => (uint)p.Hash == vehicleHandler.Model))
            {
                if (vehicleHandler.VehicleData.Inventory == null)
                    vehicleHandler.VehicleData.Inventory = new Inventory.Inventory(40, 20);

                vehicleHandler.VehicleData.TorqueMultiplicator = 20;
                vehicleHandler.VehicleData.PowerMultiplicator = 30;
            }

            return base.OnVehicleOut(client, vehicleHandler);
        }

        private void CallUrgenceLSPD(IPlayer player, object[] arg)
        {
            if (!player.Exists)
                return;

            if (PhoneManager.HasOpenPhone(player, out Phone.Phone phone))
            {
                var players = GetEmployeeOnline();

                if (players.Count > 0)
                {
                    foreach (IPlayer client in players)
                    {
                        if (client.Exists && client != player)
                            client.EmitLocked("LSPD_Call", player.Id, phone.PhoneNumber, JsonConvert.SerializeObject(client.Position.ConvertToEntityPosition()), arg[0]);
                    }
                }
                //player.EmitLocked("LSPD_Call", ServicePlayerList.Count);
            }
        }
        #endregion
    }
}
