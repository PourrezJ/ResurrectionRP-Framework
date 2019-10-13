using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Teleport;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Phone;
using ResurrectionRP_Server.Utils.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class DockItemData
    {
        public ItemID ItemID;
        public string Name;
        public int MaxQuantity;
        public double Price;
    }

    public partial class Dock : Faction
    {
        #region Public fields
        public Rack Quai;
        public Rack Importation;
        public List<Rack> Racks;

        [BsonIgnore]
        public List<Teleport.Teleport> Teleports = new List<Teleport.Teleport>();
        #endregion

        #region Constructor
        public Dock(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }
        #endregion

        #region Init
        public override Faction Init()
        {
            Teleports = new List<Teleport.Teleport>();

            ServiceLocation = new Vector3(993.1122f, -3096.144f, -38.99584f + 0.2f);
            ParkingLocation = new Location(new Vector3(1186.454f, -3200.917f, 6.101458f), new Vector3(0, 0, 254.6298f));

            FactionRang = new FactionRang[] {
                new FactionRang(0,"Ouvrier", false, 1500, false, true),
                new FactionRang(1,"Agent de sécurité", false, 1500, false, false),
                new FactionRang(2,"Chef de sécurité", false, 2000, false, false),
                new FactionRang(3,"Chef d'équipe", false, 2000, false, true),
                new FactionRang(4,"Bras droit", true, 2200, true, true),
                new FactionRang(5,"Patron", true, 2500, true, true)
            };

            VehicleAllowed = new List<FactionVehicle>();
            VehicleAllowed.Add(new FactionVehicle(0, VehicleModel.Forklift, 15000, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Benson, 400000, 200));

            // Pawn car
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Reaper, 507500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Vacca, 343000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Turismor, 472500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Infernus, 339500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Banshee, 385000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Fmj, 630000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.T20, 525000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Osiris, 560000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Tyrus, 927500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Prototipo, 1120000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Pfister811, 577500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Bullet, 332500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Voltic, 350000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Zentorno, 770000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Cheetah, 612500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.EntityXf, 402500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Le7B, 980000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Adder, 700000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Autarch, 805000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Cyclone, 525000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Emerus, 665000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Gp1, 455000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Italigtb, 525000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Krieger, 735000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Nero, 875000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Penetrator, 378000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sc1, 325500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.SultanRs, 133000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Tezeract, 1330000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Thrax, 1400000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Vagner, 1050000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Visione, 1155000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Zorrusso, 770000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodlp750, 1470000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.skyline, 108500, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.audirs6tk, 98000, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodm3e36, 133000, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rs72013, 119000, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.ben17, 157500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Ztype, 325500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.BType, 294000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Nebula, 14000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Zion3, 21000, 15));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.zl12017, 66500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodmustang, 112000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rt440, 52500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.chevelle1970, 98000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.stingray66, 108500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Hotknife, 49000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Hermes, 59500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Hustler, 47950, 10));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Dominator3, 66500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Gauntlet4, 108500, 30));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.SabreGt2, 49000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.v242, 24500, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.bmci, 94500, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.mers63c, 87500, 15));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Dubsta3, 70000, 50));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.nisaltima, 14000, 25));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Stafford, 192500, 25));
            // VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.p205t16a, 0, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.p205t16b, 70000, 25));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.charger, 54250, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodgt63, 91000, 30));
            VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodm4gts, 119000, 30));

            // Autres
            //VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.charge4, 0, 90));
            //VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rolls, 0, 25));
            //VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodx6, 0, 50));
            //VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodlp770, 0, 15));

            // Wolf Motor Shop
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Avarus, 46550, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Chimera, 122500, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Daemon, 29400, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Daemon2, 34300, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Hexer, 39200, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Innovation, 88200, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Wolfsbane, 36750, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sovereign, 73500, 7));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Zombieb, 29400, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sanctus, 100000, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Faggio2, 1470, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Diablous2, 73500, 5));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Nightblade, 122500, 5));

            // Tankers
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Tanker, 140000, 500));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Tanker2, 140000, 500));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.ArmyTanker, 140000, 500));

            // Trailers
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.TrFlat, 126000, 0));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Trailers, 133000, 500));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Trailers2, 154000, 500));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Trailers3, 154000, 500));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.TrailerLogs, 140000, 500));
            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.DockTrailer, 140000, 500));

            VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Microlight, 150000, 15));

            BlipPosition = new Vector3(1181.065f, -3113.788f, 6.028026f);
            BlipColor = BlipColor.Yellow;
            BlipSprite = 356;

            ItemShop.Add(new FactionShopItem(Inventory.Inventory.ItemByID(ItemID.Coffee), 0, 0));

            base.Init();

            // Entrée piéton bâtiment
            List<TeleportEtage> doors = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Bureau", Location = new Location(new Vector3(992.6411f, -3097.875f, -38.99584f), new Vector3(0, 0, 269.3458f))}
            };

            Teleports.Add(Teleport.Teleport.CreateTeleport(new Location(new Vector3(1181.065f, -3113.788f, 6.028026f), new Vector3(0, 0, 94.32016f)), doors, new Vector3(1,1,0.2f), true, iswhitelisted: true, menutitle: "Porte", whitelist: FactionPlayerList.Keys.ToList()));

            // Entrée chariot bâtiment
            doors = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Entrepôt", Location = new Location(new Vector3(1023.734f, -3101.668f, -39.55f), new Vector3(0.09950814f, 0.3552414f, 76.95227f))}
            };

            Teleports.Add( Teleport.Teleport.CreateTeleport(new Location(new Vector3(1189.579f, -3106.517f, 5.3f), new Vector3(-3.693874f, -0.06248324f, 358.6757f)), doors, new Vector3(1, 1, 0.2f), true, iswhitelisted: true, menutitle: "Entrepôt", whitelist: ServicePlayerList));

            Parking.Hidden = true;
            Parking.Spawn1 = new Location(new Vector3(1186.454f, -3200.917f, 6.101458f), new Vector3(0.01919898f, 0.2955396f, 88.09204f));
            Parking.Spawn2 = new Location(new Vector3(1186.677f, -3195.231f, 6.094764f), new Vector3(0.4719574f, 0.1808039f, 89.15057f));

            var pnj =  Ped.CreateNPC(PedModel.Dockwork01SMM, new Vector3(1232.77f, -3034.825f, 9.363697f), 10.35416f);
            pnj.NpcInteractCallBack += OnPedInteract;

            if (Quai == null && Importation == null && Racks == null)
                GenerateAllRackPosition();
            else
            {
                if (Quai == null)
                    Quai = Rack.CreateRack("Quai", new Vector3(1180.685f, -3168.366f, 5.1175f), new Location(new Vector3(1183.685f, -3168.366f, 7.1175f), new Vector3(0, 0, 83.16074f)), true);
                
                if (Importation == null)
                    Importation = Rack.CreateRack("Importation", new Vector3(1216.311f, -3038.197f, 4.868594f), new Location(new Vector3(1216.311f, -3041.197f, 5.868594f), new Vector3()), true);


                Quai.RackPos = new Vector3(1180.685f, -3168.366f, 4.1175f);
                Importation.RackPos = new Vector3(1216.311f, -3038.197f, 4.868594f);

                Quai.Load();
                Importation.Load();

                for (int i = 0; i < Racks.Count; i++)
                {
                    var rack = Racks[i];

                    if (rack != null)
                    {
                        Racks[i].Load();
                        Racks[i].Colshape.OnPlayerEnterColshape += OnPlayerEnterColShape;
                        Racks[i].Colshape.OnVehicleEnterColshape += OnVehicleEnterColShape;
                    }
                }
            }

            Alt.OnClient("OpenRackInventory", OpenRackInventory);

            return this;
        }
        #endregion

        #region Event handlers
        private void OnPedInteract(IPlayer client, Ped npc)
        {
            if (!HasPlayerIntoFaction(client))
            {
                client.SendNotificationError("Que faite vous sur mon bateau, cassez-vous!");
                return;
            }

            if (Importation.InventoryBox == null)
            {
                client.SendNotificationError("Vous n'avez pas mis de caisse sous la grue.");
                return;
            }

            if ((GetRangPlayer(client)) < 3)
            {
                client.SendNotificationError("Hey le larbin, retourne passer le balai.");
                return;
            }

            var listItem = new List<DockItemData>();

            foreach (var item in LoadItem.ItemsList)
            {
                if (!item.isDockable)
                    continue;

                listItem.Add(new DockItemData() { MaxQuantity = 100, Name = item.name, Price = item.itemPrice, ItemID = item.id });
            }

            OpenImportationMenu(client, listItem);
        }

        public void OnVehicleEnterColShape(IColshape colshape, IVehicle vehicle)
        {
            if (vehicle.Driver == null)
                return;
            var player = vehicle.Driver;

            for (int i = 0; i < Racks.Count; i++)
            {
                if (Racks[i].Colshape.IsEntityIn(player))
                {
                    Racks[i].OnPlayerEnterColShape(colshape, player);
                    break;
                }
            }

        }

        public override void OnPlayerPromote(IPlayer client, int rang)
        {
            foreach (var teleport in Teleports)
            {
                if (!teleport.Whileliste.Contains(client.GetSocialClub()))
                    teleport.Whileliste.Add(client.GetSocialClub());
            }

            base.OnPlayerPromote(client, rang);
        }

        public override Task OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            return base.OnVehicleOut(client, vehicle, location);
        }

        public override void PlayerFactionAdded(IPlayer client)
        {
            foreach (var teleport in Teleports)
            {
                if (!teleport.Whileliste.Contains(client.GetSocialClub()))
                    teleport.Whileliste.Add(client.GetSocialClub());
            }

            base.PlayerFactionAdded(client);
        }
        #endregion

        #region Methods
        private bool Dock_CommandeValidate(IPlayer player, Menu menu, Dictionary<DockItemData, int> importItems)
        {
            if (!player.Exists)
                return false;

            if (Importation.InventoryBox == null)
            {
                player.SendNotificationError("Le box n'est plus sous la grue.");
                return false;
            }

            int totalQuantity = 0;

            foreach (int quantity in importItems.Values)
                totalQuantity += quantity;

            if (totalQuantity > Importation.InventoryBox.Inventory.MaxSize - Importation.InventoryBox.Inventory.CurrentSize())
            {
                player.SendNotificationError("Pas assez de place dans le box pour cette commande!");
                return false;
            }

            if (!BankAccount.GetBankMoney(CalculPrice(importItems), "Importation commande"))
            {
                player.SendNotificationError("Vous n'avez pas les fonds pour cette commande!");
                return false;
            }

            foreach (KeyValuePair<DockItemData, int> item in importItems)
            {
                if (item.Value > 0)
                {
                    var itemData = LoadItem.ItemsList.Find(p => p.name == item.Key.Name);

                    if (item.Key.ItemID == ItemID.Phone)
                    {
                        for (int i = 0; i < item.Value; i++)
                            Importation.InventoryBox.Inventory.AddItem(new PhoneItem(ItemID.Phone, "Téléphone", "", PhoneManager.GeneratePhone(), 0, true, true, false, true, false), 1);
                    }
                    else if (itemData.isStackable)
                    {
                        for (int i = 0; i < item.Value; i++)
                            Importation.InventoryBox.Inventory.AddItem(itemData, 1);
                    }
                    else
                        Importation.InventoryBox.Inventory.AddItem(itemData, item.Value);
                }
            }

            UpdateInBackground();
            Importation.RefreshLabel();
            player.SendNotificationSuccess("Commande validée!");
            return true;
        }

        private double CalculPrice(Dictionary<DockItemData, int> panier)
        {
            double price = 0;
            foreach (var item in panier)
            {
                price += item.Key.Price * item.Value;
            }
            return price;
        }

        private void OpenRackInventory(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            string _id = args[0].ToString();
            var ph = player.GetPlayerHandler();

            if (ph == null)
                return;

            if (!HasPlayerIntoFaction(player))
            {
                player.SendNotificationError("Vous n'êtes pas autorisé à ouvrir cette caisse.");
                return;
            }

            RPGInventoryMenu _inv = null;

            Rack rack = null;

            if (Quai.RackName == _id)
            {
                rack = Quai;
                _inv = new RPGInventoryMenu(ph.PocketInventory, null, ph.BagInventory, Quai.InventoryBox.Inventory);
            }
            else if (Importation.RackName == _id)
            {
                rack = Importation;
                _inv = new RPGInventoryMenu(ph.PocketInventory, null, ph.BagInventory, Importation.InventoryBox.Inventory);
            }
            else if (Racks.Exists(p => p.RackName == _id))
            {
                rack = Racks.Find(p => p.RackName == _id);
                _inv = new RPGInventoryMenu(ph.PocketInventory, null, ph.BagInventory, rack.InventoryBox.Inventory);
            }

            if (_inv == null)
                return;

            _inv.OpenMenu(player);

            // Save Inventory on move
            _inv.OnMove = (IPlayer c, RPGInventoryMenu m) =>
            {
                ph.UpdateFull();
                UpdateInBackground();
                rack?.RefreshLabel();
            };

            // Save Inventory on close
            _inv.OnClose = (IPlayer c, RPGInventoryMenu m) =>
            {
                ph.UpdateFull();
                UpdateInBackground();
                rack?.RefreshLabel();
            };
        }

        public void GenerateAllRackPosition()
        {
            Racks = new List<Rack>();
            Quai = Rack.CreateRack("Quai", new Vector3(1180.685f, -3168.366f, 4.1175f), new Location(new Vector3(1183.685f, -3168.366f, 7.1175f), new Vector3(0, 0, 83.16074f)), true);
            Importation = Rack.CreateRack("Importation", new Vector3(1216.311f, -3038.197f, 3.868594f), new Location(new Vector3(1216.311f, -3041.197f, 5.868594f), new Vector3()), true);

            Vector3 a = new Vector3(1003.698f, -3111.308f, -38.99989f);
            Vector3 b = new Vector3(1018.397f, -3094.57f, -38.99988f);

            float x = (b.X - a.X) / 6;
            float y = (b.Y - a.Y) / 3;
            int e = 0;

            for (int j = 0; j < 4; j++)
            {
                Vector3 temp = new Vector3(a.X, a.Y, a.Z);
                temp.Y += j * y;

                Racks.Add(Rack.CreateRack($"Rack {e}", new Vector3(temp.X, temp.Y, temp.Z - 1), new Location(new Vector3(temp.X, temp.Y + 3, temp.Z), new Vector3())));
                e++;

                for (int i = 0; i < 6; i++)
                {
                    temp.X += x;
                    Racks.Add(Rack.CreateRack($"Rack {e}", new Vector3(temp.X, temp.Y, temp.Z - 1), new Location(new Vector3(temp.X, temp.Y + 3, temp.Z), new Vector3())));
                    e++;
                }
            }
        }
        #endregion
    }
}
