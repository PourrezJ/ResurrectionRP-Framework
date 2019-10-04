using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Illegal
{
    public class IllegalManager
    {
        public static List<IllegalSystem> IllegalList = new List<IllegalSystem>();

        public static WeedBusiness WeedBusiness { get; set; }

        public async Task OnEnterColshape(IPlayer client, IColshape colshape)
        {
            foreach (var illegal in IllegalList)
            {
                await illegal.OnEnterColshape(client, colshape);
            }
        }

        public void OnPlayerConnected(IPlayer client)
        {
            foreach (var illegal in IllegalList)
            {
                illegal.OnPlayerConnected(client);
            }
        }

        public static async Task InitAll()
        {
            Alt.Server.LogDebug("--- Start loading all illegal business in database ---");

            var _illagelBusinessesList = await Database.MongoDB.GetCollectionSafe<IllegalSystem>("illegal").AsQueryable().ToListAsync();
            foreach (var _businesses in _illagelBusinessesList)
            {
                if (_businesses.GetType() == typeof(WeedBusiness))
                    WeedBusiness = _businesses as WeedBusiness;

                

                await _businesses.Load();
                IllegalList.Add(_businesses);
            }

            Alt.Server.LogDebug($"--- Finish loading all illegal businesses in database: {_illagelBusinessesList.Count} ---");

            Utils.Utils.SetInterval(async () =>
            {
                foreach (var _businesses in IllegalList)
                {
                    if (_businesses.Enabled)
                    {
                        await _businesses.Update();
                    }
                    await Task.Delay(150);
                }
            }, (int)TimeSpan.FromMinutes(2).TotalMilliseconds);
        }
    }

    [BsonKnownTypes(typeof(WeedBusiness))]
    public class IllegalSystem
    {
        public BsonObjectId _id;

        public bool Enabled { get; set; } = false;

        public Inventory.Inventory Inventory { get; set; }

        public int CurrentPos;
        public Location[] DealerLocations;
        public DateTime NextRefreshDealerPos;
        [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
        public PedModel DealerPedHash;

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ItemID, double> IllegalPrice
            = new Dictionary<ItemID, double>();

        [BsonIgnore]
        public Ped DealerPed;

        public virtual async Task Load()
        {
            if (NextRefreshDealerPos < DateTime.Now || NextRefreshDealerPos == new DateTime())
            {
                NextRefreshDealerPos = DateTime.UtcNow.AddDays(7);
                CurrentPos = Utils.Utils.RandomNumber(0, DealerLocations.Length);
                await Update();
            }

            if (DealerLocations.Length > 0)
            {
                var loc = DealerLocations[CurrentPos];

                DealerPed = Ped.CreateNPC(DealerPedHash, loc.Pos, loc.Rot.Z, GameMode.GlobalDimension);
                Alt.Server.LogInfo($"Current Dealer {this.GetType().Name}: {loc.Pos.X} {loc.Pos.Y} {loc.Pos.Z}");
                DealerPed.NpcInteractCallBack += OnDealerInteract;
            }
        }

        public virtual void OnDealerInteract(IPlayer sender, Ped npc)
        {
            PlayerHandler player = sender.GetPlayerHandler();
            if (player.IsOnProgress || sender.IsInVehicle)
                return;

            if (GameMode.Instance.FactionManager.Lspd.ServicePlayerList.Count < 2)
            {
                sender.SendNotificationError("[HRP] Pas assez de miliciens de présent sur le serveur.");
                return;
            }

            if (IllegalPrice.Count > 0)
            {
                foreach (var ItemProcess in IllegalPrice)
                {
                    var ItemIDProcess = ItemProcess.Key;
                    var ItemPrice = ItemProcess.Value;

                    Item _itemBuy = ResurrectionRP_Server.Inventory.Inventory.ItemByID(ItemIDProcess);

                    if (!player.HasItemID(ItemIDProcess))
                        continue;
                    player.IsOnProgress = true;
                    sender.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

                    MenuManager.CloseMenu(sender);
                    int itemcount = player.CountItem(_itemBuy);

                    sender.Emit("LaunchProgressBar", 1000 * itemcount);

                    Utils.Utils.Delay(1000 * itemcount, () =>
                    {
                        if (!sender.Exists)
                            return;

                        if (player.DeleteAllItem(ItemIDProcess, itemcount))
                        {
                            player.AddMoney(ItemPrice * itemcount);
                            sender.DisplaySubtitle($"~r~{itemcount} ~w~{_itemBuy.name}(s) ~r~${(ItemPrice * itemcount)}~w~.", 15000);
                        }
                        else
                            sender.SendNotificationError("Inconnu.");

                        player.IsOnProgress = false;
                        player.UpdateFull();
                    });
                }
            }
        }

        public virtual Task OnEnterColshape(IPlayer client, IColshape colshape)
        {
            return Task.CompletedTask;
        }

        public async Task Update()
        {
            await Database.MongoDB.Update(this, "illegal", _id);
        }

        public virtual void OnPlayerConnected(IPlayer client)
        {
        }
    }
}