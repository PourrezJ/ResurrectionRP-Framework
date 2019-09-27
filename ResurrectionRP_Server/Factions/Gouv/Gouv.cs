using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.XMenuManager;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;

namespace ResurrectionRP_Server.Factions
{

    public partial class Gouv : Faction
    {
        #region Private fields
        private List<Door> _doors;
        private Teleport.Teleport _teleport;
        // private IColShape _portail1;
        // private IColShape _portail2;
        private IColShape _boatColShape;
        private Ped _secretaire;
        #endregion

        #region Public properties
        public Door PortChancelierA { get; private set; }

        public Door PortChancelierB { get; private set; }

        public Door PortDevantA { get; private set; }

        public Door PortDevantB { get; private set; }

        public Door PortDerriereA { get; private set; }

        public Door PortDerriereB { get; private set; }

        public Door PortailA { get; private set; }

        public Door PortailB { get; private set; }

        public Inventory.Inventory BoatInventory { get; private set; }
        #endregion

        #region Constructor
        public Gouv(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
            BankAccount = new Bank.BankAccount(Bank.AccountType.Faction, "Gouvernement", 500000000);
        }
        #endregion

        #region Init
        public override Faction Init()
        {

            ParkingLocation = new Location(new Vector3(-580.5234f, -170.6517f, 37.02826f), new Vector3(-1.193955f, 0.651089f, 295.0412f));
            HeliportLocation = new Location(new Vector3(-543.1163f, -252.641f, 37.25359f), new Vector3(0, 0, 198.6213f));
            ServiceLocation = new Vector3(-541.8453f, -193.0201f, 47.42305f);
            ShopLocation = new Vector3(-551.4822f, -186.3922f, 37.22308f);

            FactionRang = new FactionRang[] {
                new FactionRang(0,"Chauffeur", false, 1500, false, false),
                new FactionRang(1,"Sécurité ", false, 2000, false, false),
                new FactionRang(2,"Chef de sécurité ", false, 3000, false, true),
                new FactionRang(3,"Agent gouvernemental ", false, 3000, false, true),
                new FactionRang(4,"Conseiller ", true, 3500, false, true),
                new FactionRang(5,"Ministre ", true, 4500, true, true),
                new FactionRang(6,"Ambassadeur", true, 6000, true, true),
                new FactionRang(7,"Chancelier ", true, 8000, true, true),
            };

            BlipPosition = new Vector3(-544.775f, -204.4195f, 38.21515f);
            BlipColor = (BlipColor)75;
            BlipSprite = 537;

            PortChancelierA = Door.CreateDoor(110411286, new Vector3(-549.8286f, -196.6932f, 47.41496f), true);
            PortChancelierB = Door.CreateDoor(110411286, new Vector3(-548.8829f, -195.8936f, 47.41496f), true);

            PortDevantA = Door.CreateDoor(2537604, new Vector3(-545.2567f, -202.7703f, 38.22255f), true);
            PortDevantB = Door.CreateDoor(2537604, new Vector3(-545.9545f, -203.4294f, 38.2224f), true);

            PortDerriereA = Door.CreateDoor(114775988, new Vector3(-582.8537f, -196.2415f, 38.23097f), true);
            PortDerriereB = Door.CreateDoor(114775988, new Vector3(-582.0195f, -195.5236f, 38.23092f), true);

            // Villa Chancelier
            PortailA = Door.CreateDoor(3945237283, new Vector3(-123.8705f, 899.3705f, 235.792f), true);
            PortailB = Door.CreateDoor(2376486946, new Vector3(-125.1007f, 902.032f, 235.7901f), true);

            _doors = new List<Door>()
            {
                PortChancelierA,
                PortChancelierB,
                PortDevantA,
                PortDevantB,
                PortailA,
                PortailB,
                PortDerriereA,
                PortDerriereB
            };
            foreach (var door in _doors)
                door.Interact = OpenCelluleDoor;

            _secretaire = Ped.CreateNPC(PedModel.Bevhills03AFY, new Vector3(-550.1015f, -190.1574f, 38.22381f), 192.4752f);
            _secretaire.NpcInteractCallBack = OnNPCInteract;

            Ped npcsecuoutside = Ped.CreateNPC(PedModel.ChemSec01SMM, new Vector3(-519.5601f, -257.3863f, 35.76096f), 299.4756f);

            VehicleAllowed.Add(new FactionVehicle(6, VehicleModel.Swift, 5000000, 25));
            VehicleAllowed.Add(new FactionVehicle(2, VehicleModel.Menacer, 2000000, 90));
            VehicleAllowed.Add(new FactionVehicle(2, VehicleModel.Baller5, 700000, 90));
            VehicleAllowed.Add(new FactionVehicle(2, VehicleModel.Cog552, 400000, 60));
            VehicleAllowed.Add(new FactionVehicle(2, VehicleModel.Schafter6, 400000, 90));
            VehicleAllowed.Add(new FactionVehicle(3, VehicleModel.Kuruma2, 1500000, 60));
            //VehicleAllowed.Add(new FactionVehicle(3, (VehicleModel)VehicleHash2.rmodx6, 1500000, 60)); Véhicule moddé ?

            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Flashlight, "Lampe de poche", "Une lampe leds 500watts.", hash: WeaponHash.Flashlight), 0, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Matraque, "Matraque", "Une matraque de marque Théo.", hash: WeaponHash.Nightstick), 0, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Pistol, "Pistol MK2", "", hash: WeaponHash.PistolMk2), 0, 1));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Pistol, "Revolver MK2", "", hash: WeaponHash.RevolverMk2), 0, 2));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Pump, "Fusil à canon scié", "", hash: WeaponHash.SawnOffShotgun), 0, 2));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Carabine, "Special Carbine MK2", "", hash: WeaponHash.SpecialCarbineMk2), 0, 3));
            ItemShop.Add(new FactionShopItem(new RadioItem(ItemID.Radio, "Talky", "", 1, true, true, false, true, true, 2000, icon: "talky"), 500, 0));
            ItemShop.Add(new FactionShopItem(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(45, 0, 0), 40, 20, 1, true, false, false, true, true, 500, classes: "backpack", icon: "backpack"), 500, 0));
            ItemShop.Add(new FactionShopItem(new HandCuff(ItemID.Handcuff, "Menottes", "Une paire de menottes", 0.1, true, false), 500, 0));
            ItemShop.Add(new FactionShopItem(new Weapons(ItemID.Weapon, "Combat MG MK2", "", 26, hash: WeaponHash.CombatMgMk2), 40000, 2));

            _boatColShape = Alt.CreateColShapeCylinder(new Vector3(-2095.39f, -1014.713f, 8.980465f), 4f, 4f);

            if (BoatInventory == null)
                BoatInventory = new Inventory.Inventory(500, 40);

            return base.Init();
        }
        #endregion

        #region Event handlers
        public override XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            xmenu.SetData("Player", target);

            return base.InteractPlayerMenu(client, target, xmenu);
        }

        private Task OnNPCInteract(IPlayer client, Ped npc)
        {
            if (npc == _secretaire)
                OpenSecretaryMenu(client);

            return Task.CompletedTask;
        }

        public override Task OnVehicleOut(IPlayer client, VehicleHandler vehicleHandler, Location location = null)
        {
            if (VehicleAllowed.Exists(p => (uint)p.Hash == vehicleHandler.Model))
            {
                if (vehicleHandler.Inventory == null)
                    vehicleHandler.Inventory = new Inventory.Inventory(40, 20);
            }

            return base.OnVehicleOut(client, vehicleHandler, location);
        }

        public override void OnPlayerEnterColShape(IColShape ColShapePointer, IPlayer player)
        {
            if (_boatColShape == ColShapePointer)
            {
                Menu menu = new Menu("", "", "", backCloseMenu: true);
                menu.ItemSelectCallbackAsync = MenuCallback;
                menu.Add(new MenuItem("Inventaire", "", "ID_Inventaire", true));

                Task.Run(async () => { await menu.OpenMenu(player); }).Wait();
            }

            base.OnPlayerEnterColShape(ColShapePointer, player);
        }

        public override async Task OnPlayerPromote(IPlayer client, int rang)
        {
            if (!_teleport.Whileliste.Contains(client.GetSocialClub()))
                _teleport.Whileliste.Add(client.GetSocialClub());
            await base.OnPlayerPromote(client, rang);
        }

        public override async Task PlayerFactionAdded(IPlayer client)
        {
            if (!_teleport.Whileliste.Contains( client.GetSocialClub()))
                _teleport.Whileliste.Add( client.GetSocialClub());
            await base.PlayerFactionAdded(client);
        }
        #endregion

        #region Methods
        private async Task OpenCelluleDoor(IPlayer client, Door door)
        {
            if (HasPlayerIntoFaction(client))
            {
                XMenu xmenu = new XMenu("ID_Door");
                xmenu.SetData("Door", door);

                if ( GetRangPlayer(client) >= 5 && (door == PortChancelierA || door == PortChancelierB))
                {
                    XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                    item.OnMenuItemCallback = OnDoorCall;
                    xmenu.Add(item);
                }
                else if ( GetRangPlayer(client) < 5 && (door == PortChancelierA || door == PortChancelierB))
                {
                    return;
                }
                else if ((door == PortailA || door == PortailB) && ( client.GetSocialClub() == "Armex72"))
                {
                    XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                    item.OnMenuItemCallback = OnDoorCall;
                    xmenu.Add(item);
                }
                else
                {
                    XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                    item.OnMenuItemCallback = OnDoorCall;
                    xmenu.Add(item);
                }

                await xmenu.OpenXMenu(client);
            }
        }

        private static async Task OnDoorCall(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            Door door = menu.GetData("Door");
            if (door != null)
            {
                door.SetDoorLockState(!door.Locked);
            }

            await XMenuManager.XMenuManager.CloseMenu(client);
        }

        private async Task MenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if ( HasPlayerIntoFaction(client))
            {
                if (menuItem.Id == "ID_Inventaire")
                {
                    var inv = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, BoatInventory);
                    inv.OnMove += async (cl, inventaire) =>
                    {
                        ph.UpdateFull();
                        await UpdateDatabase();
                    };
                    await inv.OpenMenu(client);
                }
            }
        }
        #endregion
    }
}
