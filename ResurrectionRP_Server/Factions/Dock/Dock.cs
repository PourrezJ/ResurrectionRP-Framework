using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System.Linq;
using System.Numerics;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Teleport;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Phone;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Factions.Dock
{

    public partial class Dock : Faction
    {
        public Rack Quai;
        public Rack Importation;
        public List<Rack> Racks;

        [BsonIgnore]
        public List<Teleport.Teleport> Teleports = new List<Teleport.Teleport>();

        public Dock(string FactionName, FactionType FactionType) : base(FactionName, FactionType)
        {
        }

        public override async Task<Faction> OnFactionInit()
        {
            Teleports = new List<Teleport.Teleport>();

            this.ServiceLocation = new Vector3(993.1122f, -3096.144f, -38.99584f);
            this.ParkingLocation = new Location(new Vector3(1186.454f, -3200.917f, 6.101458f), new Vector3(0, 0, 254.6298f));

            this.FactionRang = new FactionRang[] {
                new FactionRang(0,"Ouvrier", false, 1500, false, true),
                new FactionRang(1,"Agent de sécurité", false, 1500, false, false),
                new FactionRang(2,"Chef de sécurité", false, 2000, false, false),
                new FactionRang(3,"Chef d'équipe", false, 2000, false, true),
                new FactionRang(4,"Bras droit", true, 2200, true, true),
                new FactionRang(5,"Patron", true, 2500, true, true)
            };

            this.VehicleAllowed = new List<FactionVehicle>();
            this.VehicleAllowed.Add(new FactionVehicle(0, VehicleModel.Forklift, 15000, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Benson, 400000, 200));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.supra2, 600000, 25)); 
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.oiltanker, 150000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.chevelle1970, 86000, 30));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.stingray66, 83000, 30));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.bmci, 330000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.skyline, 330000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.mers63c, 100000, 15));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.zl12017, 90000, 30));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.tts, 170000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodm4gts, 600000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.focusrs, 265000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.ben17, 166000, 30));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.audirs6tk, 265000, 25));
            //this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.ast, 265000, 25)); Deja commente
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodmustang, 600000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodm3e36, 530000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodgt63, 550000, 50));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodlp770, 1000000, 15));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.rmodlp750, 1000000, 15));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.p205t16a, 360000, 25));
            this.VehicleAllowed.Add(new FactionVehicle(1, (VehicleModel)VehicleModel2.p205t16b, 330000, 25));


            // Wolf Motor Shop
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Avarus, 46550, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Chimera, 122500, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Daemon, 29400, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Daemon2, 34300, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Hexer, 39200, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Innovation, 88200, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Wolfsbane, 36750, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sovereign, 73500, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Zombieb, 29400, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Sanctus, 100000, 5));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Faggio2, 1470, 5));

            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Zentorno, 750000, 15));
            this.VehicleAllowed.Add(new FactionVehicle(1, VehicleModel.Microlight, 150000, 15));



            this.BlipPosition = new Vector3(1181.065f, -3113.788f, 6.028026f);
            this.BlipColor = BlipColor.Yellow;
            this.BlipSprite = 356;

            this.ItemShop.Add(new FactionShop(Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.Cafe), 0, 0));

            await base.OnFactionInit();

            // Entrée piéton bâtiment
            List<TeleportEtage> doors = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Bureau", Location = new Location(new Vector3(992.6411f, -3097.875f, -38.99584f), new Vector3(0, 0, 269.3458f))}
            };

            Teleports.Add(Teleport.Teleport.CreateTeleport(new Location(new Vector3(1181.065f, -3113.788f, 6.028026f), new Vector3(0, 0, 94.32016f)), doors, 1, true, iswhitelisted: true, menutitle: "Porte", whitelist: FactionPlayerList.Keys.ToList()));

            // Entrée chariot bâtiment
            doors = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Entrepôt", Location = new Location(new Vector3(1023.734f, -3101.668f, -39.55f), new Vector3(0.09950814f, 0.3552414f, 76.95227f))}
            };

            Teleports.Add( Teleport.Teleport.CreateTeleport(new Location(new Vector3(1189.579f, -3106.517f, 5.3f), new Vector3(-3.693874f, -0.06248324f, 358.6757f)), doors, 1, true, iswhitelisted: true, menutitle: "Entrepôt", whitelist: this.ServicePlayerList));

            Parking.Hidden = true;
            Parking.Spawn1 = new Location(new Vector3(1186.454f, -3200.917f, 6.101458f), new Vector3(0.01919898f, 0.2955396f, 88.09204f));
            Parking.Spawn2 = new Location(new Vector3(1186.677f, -3195.231f, 6.094764f), new Vector3(0.4719574f, 0.1808039f, 89.15057f));

            var pnj =  Ped.CreateNPC(PedModel.Dockwork01SMM,Streamer.Data.PedType.Human, new Vector3(1232.77f, -3034.825f, 9.363697f), 10.35416f);
            pnj.NpcInteractCallBack += OnPedInteract;

            if (Quai == null && Importation == null && Racks == null)
            {
                await GenerateAllRackPosition();
                await UpdateDatabase();
            }
            else
            {
                await Quai.Load();
                await Importation.Load();
                for (int i = 0; i < Racks.Count; i++)
                {
                    var rack = Racks[i];
                    if (rack != null)
                    {
                        await Racks[i].Load();
                        await Task.Delay(25);
                    }
                }
            }

            AltAsync.OnClient("OpenRackInventory", OpenRackInventory);

            return this;
        }

        private async Task<bool> Dock_CommandeValidate(IPlayer player, Menu menu, Dictionary<DockItemData, int> importItems)
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

            if (!await BankAccount.GetBankMoney(CalculPrice(importItems), "Importation commande"))
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

            await UpdateDatabase();
             Importation.RefreshLabel();
             player.SendNotificationSuccess("Commande validée!");
            return true;
        }

        private async Task OnPedInteract(IPlayer client, Ped npc)
        {
            if (! HasPlayerIntoFaction(client))
            {
                 client.SendNotificationError("Que faite vous sur mon bateau, cassez-vous!");
                return;
            }

            if (Importation.InventoryBox == null)
            {
                 client.SendNotificationError("Vous n'avez pas mis de caisse sous la grue.");
                return;
            }

            if (( this.GetRangPlayer(client)) < 3)
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

            await OpenImportationMenu(client, listItem);
            /// await client.CallAsync("OpenImportMenu", JsonConvert.SerializeObject(listItem));
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

        #region Inventory
        private async Task OpenRackInventory(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            string _id = args[0].ToString();
            var ph = player.GetPlayerHandler();

            if (ph == null)
                return;

            if (! HasPlayerIntoFaction(player))
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

            await _inv.OpenMenu(player);

            // Save Inventory on move
            _inv.OnMove = async (IPlayer c, RPGInventoryMenu m) =>
            {
                await ph.Update();
                await UpdateDatabase();
                rack?.RefreshLabel();
            };

            // Save Inventory on close
            _inv.OnClose = async (IPlayer c, RPGInventoryMenu m) =>
            {
                await ph.Update();
                await UpdateDatabase();
                rack?.RefreshLabel();
            };
        }

        #endregion

        #region Events
        public override async Task OnPlayerEnterColShape(IColShape colShape, IPlayer player)
        {
            if (Importation.Colshape == colShape)
            {
                await Importation.OnPlayerEnterColShape(colShape, player);
            }
            else if (Quai.Colshape == colShape)
            {
                await Quai.OnPlayerEnterColShape(colShape, player);
            }
            else
            {
                for (int i = 0; i < Racks.Count; i++)
                {
                    if (Racks[i].Colshape == colShape)
                    {
                        await Racks[i].OnPlayerEnterColShape(colShape, player);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Methods
        public async Task GenerateAllRackPosition()
        {
            Racks = new List<Rack>();
            Quai = await Rack.CreateRackAsync("Quai", new Vector3(1180.685f, -3168.366f, 6.1175f), new Location(new Vector3(1183.685f, -3168.366f, 7.1175f), new Vector3(0, 0, 83.16074f)), true);
            Importation = await Rack.CreateRackAsync("Importation", new Vector3(1216.311f, -3038.197f, 5.868594f), new Location(new Vector3(1216.311f, -3041.197f, 5.868594f), new Vector3()), true);

            Vector3 a = new Vector3(1003.698f, -3111.308f, -38.99989f);
            Vector3 b = new Vector3(1018.397f, -3094.57f, -38.99988f);

            float x = (b.X - a.X) / 6;
            float y = (b.Y - a.Y) / 3;
            int e = 0;

            for (int j = 0; j < 4; j++)
            {
                Vector3 temp = new Vector3(a.X, a.Y, a.Z);
                temp.Y += j * y;

                Racks.Add(await Rack.CreateRackAsync($"Rack {e}", new Vector3(temp.X, temp.Y, temp.Z - 1), new Location(new Vector3(temp.X, temp.Y + 3, temp.Z), new Vector3())));
                e++;
                for (int i = 0; i < 6; i++)
                {
                    temp.X += x;
                    Racks.Add(await Rack.CreateRackAsync($"Rack {e}", new Vector3(temp.X, temp.Y, temp.Z - 1), new Location(new Vector3(temp.X, temp.Y + 3, temp.Z), new Vector3())));
                    e++;
                }
            }
        }

        public override async Task OnPlayerPromote(IPlayer client, int rang)
        {
            foreach (var teleport in Teleports)
            {
                if (!teleport.Whileliste.Contains( client.GetSocialClub()))
                    teleport.Whileliste.Add( client.GetSocialClub());
            }
            await base.OnPlayerPromote(client, rang);
        }

        public override async Task PlayerFactionAdded(IPlayer client)
        {
            foreach (var teleport in Teleports)
            {
                if (!teleport.Whileliste.Contains(client.GetSocialClub()))
                    teleport.Whileliste.Add(client.GetSocialClub());
            }
            await base.PlayerFactionAdded(client);
        }

        public override Task OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            return base.OnVehicleOut(client, vehicle, location);
        }
        #endregion
    }

    public class DockItemData
    {
        public Models.InventoryData.ItemID ItemID;
        public string Name;
        public int MaxQuantity;
        public double Price;
    }
}
