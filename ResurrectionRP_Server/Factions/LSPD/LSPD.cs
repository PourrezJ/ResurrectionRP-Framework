using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Items;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Async;

namespace ResurrectionRP_Server.Factions
{

    public partial class LSPD : Faction
    {
        public Door Cellule1;
        public Door Cellule2;
        public Door Cellule3;
        public Door Cellule4;

        private IColShape colShapePortique;
        //private IObject _obj;

        public List<Invoice> InvoiceList = new List<Invoice>();
        private Ped armurerie;
        private Ped accueil;
        private Ped portiqueSecu;

        public LSPD(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }

        public override async Task<Faction> OnFactionInit()
        {
            FactionRang = new FactionRang[] {
                new FactionRang(0,"Cadet", false, 1500),
                new FactionRang(1,"Officier ", false, 2000),
                new FactionRang(2,"Sergent ", false, 3000),
                new FactionRang(3,"Lieutenant ", false, 3500, true, true),
                new FactionRang(4,"Capitaine ", true, 4000, true, true),
            };

            BlipPosition = new Vector3(447.2328f, -984.3354f, 30.68967f);
            BlipColor = (BlipColor)38;
            BlipSprite = 487;

            ServiceLocation = new Vector3(456.8493f, -988.7028f, 30.68966f);
            ParkingLocation = new Location(new Vector3(463.0348f, -1019.664f, 28.01235f), new Vector3(0.1801852f, 0.05430178f, 88.83884f));
            HeliportLocation = new Location(new Vector3(449.8932f, -980.8608f, 43.59076f), new Vector3(-0.006497014f, -0.05629658f, 2.130585f));
            ShopLocation = new Vector3(419.9424f, 4809.9f, -60.99793f);

            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Police3, 90000));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sheriff2, 300000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Riot, 250000));
            VehicleAllowed.Add(new FactionVehicle(2, VehicleModel.Policeb, 200000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Maverick, 900000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Fbi, 250000));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Police4, 50000));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Policeb, 110000));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Bf400, 110000));

            ItemShop.Add(new FactionShop(new HandCuff(ItemID.Handcuff, "Menottes", "Une paire de menottes", 0.1, true, false), 500, 0));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.LampeTorche, "Lampe Torche", "Une lampe leds 500watts.", 2, hash: WeaponHash.Flashlight), 2500, 0));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Matraque, "Matraque", "Une matraque de marque Théo.", 5, hash: WeaponHash.Nightstick), 3500, 0));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Pistolet MK2", "", 3, hash: WeaponHash.PistolMk2), 40000, 1));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Taser, "Taser", "", 3, hash: WeaponHash.StunGun), 30000, 0));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Flare Gun", "", 5, hash: WeaponHash.FlareGun), 20000, 1));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Revolver MK2", "", 4, hash: WeaponHash.RevolverMk2), 50000, 2));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Assault SMG", "", 10, hash: WeaponHash.AssaultSmg), 125000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "SMG", "", 10, hash: WeaponHash.AssaultSmg), 100000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Pump, "Fusil à pompe MK2", "", 10, hash: WeaponHash.PumpShotgun), 75000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Pump, "Assault Pompe", "", 16, hash: WeaponHash.AssaultShotgun), 125000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Pump, "Bull-Pup Shotgun", "", 17, hash: WeaponHash.BullpupShotgun), 110000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Carabine, "Carabine", "", 15, hash: WeaponHash.CarbineRifle), 120000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Carabine, "Special Carabine", "", 15, hash: WeaponHash.SpecialCarbine), 160000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Carabine, "Sniper Rifle", "", 15, hash: WeaponHash.SniperRifle), 300000, 3));

            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Combat MG", "", 26, hash: WeaponHash.CombatMg), 350000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Combat MG MK2", "", 26, hash: WeaponHash.CombatMgMk2), 400000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Micro SMG", "", 26, hash: WeaponHash.MicroSmg), 150000, 3));
            ItemShop.Add(new FactionShop(new ClothItem(ItemID.Hats, "Béret Homme", "", new ClothData(106, 3, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 500, 0));
            ItemShop.Add(new FactionShop(new ClothItem(ItemID.Hats, "Béret Femme", "", new ClothData(105, 3, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"), 500, 0));

            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "MarksMan Sniper MK2", "", 26, hash: WeaponHash.MarksmanRifleMk2), 250000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Rocket Launcher", "", 26, hash: WeaponHash.HomingLauncher), 400000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "RPG", "", 26, hash: WeaponHash.HomingLauncher), 400000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Grenade Launcher", "", 20, hash: WeaponHash.GrenadeLauncher), 350000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Grenade Launcher Smoke", "", 20, hash: WeaponHash.GrenadeLauncherSmoke), 250000, 3));
            ItemShop.Add(new FactionShop(new Weapons(ItemID.Weapon, "Grenade", "", 20, hash: WeaponHash.Grenade), 100000, 3));
            ItemShop.Add(new FactionShop(new RadioItem(ItemID.Radio, "Talky", "", 1, true, true, false, true, true, 2000, icon: "talky"), 500, 0));

            ItemShop.Add(new FactionShop(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(45, 0, 0), 40, 20, 1, true, false, false, true, true, 500, classes: "backpack", icon: "backpack"), 500, 0));

            #region Portique

            colShapePortique = Alt.CreateColShapeCylinder(new Vector3(438.4902f, -981.9595f, 30.76797f), 1f,  2f);
            //_obj = await MP.Objects.NewAsync(MP.Utility.Joaat("hei_prop_carrier_docklight_01"), new Vector3(PortiquePos.X, PortiquePos.Y, PortiquePos.Z + 2), new Vector3());
            //await _obj.SetAlphaAsync(0);
            #endregion

            #region Peds
            //portiqueSecu = Ped.CreateNPC(PedModel.Cop01SFY, new Vector3(438.8754f, -985.9456f, 30.68967f), 95.78963f);
            //portiqueSecu.NpcInteractCallBack = OnNPCInteract;

            accueil = Ped.CreateNPC(PedModel.Cop01SMY, new Vector3(441.4182f, -978.3267f, 30.68967f), 176.1195f);
            accueil.NpcInteractCallBack = OnNPCInteract;

            armurerie = Ped.CreateNPC(PedModel.Cop01SMY, new Vector3(454.0918f, -979.9051f, 30.68966f), 96.70792f);
            armurerie.NpcInteractCallBack = OnNPCInteract;
            #endregion

            #region Doors
            Cellule1 = Door.CreateDoor(631614199, new Vector3(461.8065f, -994.4086f, 25.06443f), true);
            Cellule2 = Door.CreateDoor(631614199, new Vector3(461.8065f, -997.6583f, 25.06443f), true);
            Cellule3 = Door.CreateDoor(631614199, new Vector3(461.8065f, -1001.302f, 25.06443f), true);
            Cellule4 = Door.CreateDoor(631614199, new Vector3(464.5701f, -992.6641f, 25.06443f), true);

            var doors = new List<Door>()
            {
                Cellule1,
                Cellule2,
                Cellule3,
                Cellule4,
                Door.CreateDoor(320433149, new Vector3(434.7479f, -983.2151f, 30.83926f), false, false),     // Porte devant
                Door.CreateDoor(-1215222675, new Vector3(434.7479f, -980.6184f, 30.83926f), false, false),    // Porte devant
                Door.CreateDoor(-2023754432, new Vector3(469.9679f, -1014.452f, 26.53623f), true, false),    // Porte arrière
                Door.CreateDoor(-2023754432, new Vector3(467.3716f, -1014.452f, 26.53623f), true, false),    // Porte arrière            
                Door.CreateDoor(749848321, new Vector3(453.0793f, -983.1895f, 30.83926f), true, false),    // Armurerie
                Door.CreateDoor(749848321, new Vector3(461.2865f, -985.3206f, 30.83926f), true, false), // Toit
                Door.CreateDoor(-131296141, new Vector3(443.0298f, -991.941f, 30.8393f), true, false), // Briefing
                Door.CreateDoor(-1320876379, new Vector3(446.5728f, -980.0106f, 30.8393f), true, false), // bureau
                Door.CreateDoor(3079744621, new Vector3(445.3772f, -998.8503f, 30.73355f), false, false), // Porte garage
                Door.CreateDoor(320433149, new Vector3(446.853f, -998.8173f, 30.73125f), false, false),    // Porte garage
            };

            foreach (var door in doors)
                door.Interact = OpenCelluleDoor;
            #endregion

            #region Clothings

            #endregion

            Alt.OnClient("CallUrgenceLSPD", CallUrgenceLSPD);
            

            return await base.OnFactionInit();
        }

        private async Task OnNPCInteract(IPlayer client, Ped npc)
        {
            if (npc == armurerie)
            {
                await this.OpenShopMenu(client);
            }
            else if (npc == accueil)
            {
                await this.OpenAccueilMenu(client);
            }
        }

        public override Task OnPlayerServiceEnter(IPlayer client, int rang)
        {
            // Accès aux docks
            /*            foreach (var teleport in GameMode.Instance.FactionManager.Dock.Teleports) TODO
                        {
                            teleport.Whileliste.Add( client.GetSocialClub());
                        };*/
            return Task.CompletedTask;
        }

        public override async Task OnPlayerServiceQuit(IPlayer client, int rang)
        {
            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            var social =  client.GetSocialClub();

            // Accès aux docks
/*            foreach (var teleport in GameMode.Instance.FactionManager.Dock.Teleports) TODO
            {
                if (teleport.Whileliste.Contains(social))
                    teleport.Whileliste.Remove(social);
            };*/

            await base.OnPlayerServiceQuit(client, rang);
        }

        public override Task OnPlayerConnected(IPlayer client)
        {
            return Task.CompletedTask;
        }

        public override Task OnPlayerEnterColShape(IColShape colshape,IPlayer client)
        {
            return Task.CompletedTask;
            /*
            try
            {
                if (ColShapePortique == null) return;
                if (colshapePointer == ColShapePortique)
                {
                    var ph = PlayerManager.GetPlayerByClient(client);

                    if (ph == null)
                        return;

                    var itemAlert = new List<ItemID>()
                    {
                        ItemID.Weapon,
                        ItemID.LampeTorche,
                        ItemID.Carabine,
                        ItemID.Matraque,
                        ItemID.Bat,
                        ItemID.BattleAxe,
                        ItemID.CombatPistol,
                        ItemID.Flashlight,
                        ItemID.Hache,
                        ItemID.HeavyPistol,
                        ItemID.Knife,
                        ItemID.Machete,
                        ItemID.Musket,
                        ItemID.SNSPistol,
                        ItemID.Colt6Coup,
                        ItemID.Colt1911,
                        ItemID.Magnum357,
                        ItemID.Pistol50,
                        ItemID.Pistol,
                        ItemID.Pump,
                        ItemID.Taser
                    };

                    if (ph.GetAllItems().Exists(p => itemAlert.Contains(p.Item.id)))
                    {
                        var receverList = await MP.Players.GetInRangeAsync(client.Position, 5f, client.Dimension);

                        if (receverList == null)
                            return;

                        foreach (IPlayer recever in receverList)
                        {
                            if (_obj != null && recever.Exists)
                            {
                                await recever.PlaySoundFromEntity(_obj, 0, "Bomb_Disarmed", "GTAO_Speed_Convoy_Soundset");
                                await Task.Delay(300);
                                await recever.PlaySoundFromEntity(_obj, 0, "Bomb_Disarmed", "GTAO_Speed_Convoy_Soundset");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //MP.Logger.Error("LSPD Colshape", ex);
            }*/
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
                     //new ClothItem(ItemID.Hats, "", "", new ClothData(90, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     //new ClothItem(ItemID.Jacket, "", "", new ClothData(220, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     //new ClothItem(ItemID.TShirt, "", "", new ClothData(15, 0, 0), 0, false, false, false, false, false, 0, classes: "shirt", icon: "shirt"),
                     //new ClothItem(ItemID.Pant, "", "", new ClothData(33, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     //new ClothItem(ItemID.Shoes, "", "", new ClothData(24, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                    //new ClothItem(ItemID.Glove, "", "", new ClothData(4, 0, 0), 0, false, false, false, false, false, 0, classes: "gloves", icon: "gloves")

                     new ClothItem(ItemID.Hats, "", "", new ClothData(106, 3, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(90, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(220, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     //new ClothItem(ItemID.TShirt, "", "", new ClothData(15, 0, 0), 0, false, false, false, false, false, 0, classes: "shirt", icon: "shirt"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(77, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(25, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                     //new ClothItem(ItemID.Glove, "", "", new ClothData(4, 0, 0), 0, false, false, false, false, false, 0, classes: "gloves", icon: "gloves"),
                     new ClothItem(ItemID.Glasses, "", "", new ClothData(2, 0, 0), 0, false, false, false, false, false, 0, classes: "glasses", icon: "glasses"),
                     new ClothItem(ItemID.Mask, "", "", new ClothData(51, 0, 0), 0, false, false, false, false, false, 0, classes: "mask", icon: "mask"),
                     new ClothItem(ItemID.Kevlar, "", "", new ClothData(28, 1, 0), 0, false, false, false, false, false, 0, classes: "kevlar", icon: "kevlar"),
                };
            }
            else
            {
                clothItem = new List<ClothItem>()
                {
                    new ClothItem(ItemID.Hats, "", "", new ClothData(105, 3, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(107, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Hats, "", "", new ClothData(89, 0, 0), 0, false, false, false, false, false, 0, classes: "cap", icon: "cap"),
                     new ClothItem(ItemID.Jacket, "", "", new ClothData(218, 0, 0), 0, false, false, false, false, false, 0, classes: "jacket", icon: "jacket"),
                     //new ClothItem(ItemID.TShirt, "", "", new ClothData(15, 0, 0), 0, false, false, false, false, false, 0, classes: "shirt", icon: "shirt"),
                     new ClothItem(ItemID.Pant, "", "", new ClothData(79, 0, 0), 0, false, false, false, false, false, 0, classes: "pants", icon: "pants"),
                     new ClothItem(ItemID.Shoes, "", "", new ClothData(25, 0, 0), 0, false, false, false, false, false, 0, classes: "shoes", icon: "shoes"),
                     //new ClothItem(ItemID.Glove, "", "", new ClothData(7, 0, 0), 0, false, false, false, false, false, 0, classes: "gloves", icon: "gloves"),
                     new ClothItem(ItemID.Glasses, "", "", new ClothData(9, 1, 0), 0, false, false, false, false, false, 0, classes: "glasses", icon: "glasses"),
                     new ClothItem(ItemID.Mask, "", "", new ClothData(51, 0, 0), 0, false, false, false, false, false, 0, classes: "mask", icon: "mask"),
                     new ClothItem(ItemID.Kevlar, "", "", new ClothData(30, 1, 0), 0, false, false, false, false, false, 0, classes: "kevlar", icon: "kevlar"),
                };
            }


            for (int i = 0; i < clothItem.Count; i++)
            {
                var cloth = clothItem[i];
                FactionPlayerList[ client.GetSocialClub()].Inventory.AddItem(cloth, 1);
            }

            await base.PlayerFactionAdded(client);
        }

        public override Task OnVehicleOut(IPlayer client, VehicleHandler vehicleHandler, Location location)
        {
            if (VehicleAllowed.Exists(p => (uint)p.Hash == vehicleHandler.Model))
            {
                if (vehicleHandler.Inventory == null)
                    vehicleHandler.Inventory = new Inventory.Inventory(40, 20);

                vehicleHandler.TorqueMultiplicator = 20;
                vehicleHandler.PowerMultiplicator = 30;
            }

            return base.OnVehicleOut(client, vehicleHandler);
        }

        private void CallUrgenceLSPD(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (Phone.PhoneManager.HasOpenPhone(client, out Phone.Phone phone))
            {
                Phone.PhoneManager.ClosePhone(client);

                var players =  GetEmployeeOnline();

                if (players.Count > 0)
                {
                    foreach (IPlayer player in players)
                    {
                        if (player.Exists)
                            player.EmitLocked("LSPD_Call", client, phone.PhoneNumber, JsonConvert.SerializeObject(client.Position.ConvertToEntityPosition()), args[0]);
                    }
                }
                //client.EmitLocked("LSPD_Call", ServicePlayerList.Count);
            }
        }
    }
}
