using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class FarmManager
    {
        #region Static fields
        public static List<Farm> FarmList = new List<Farm>();
        #endregion

        #region Init
        public static FarmManager InitAll()
        {
            var farmManager = new FarmManager();
            var validator_type = typeof(Farm);

            var sub_validator_types =
                validator_type
                .Assembly
                .DefinedTypes
                .Where(x => validator_type.IsAssignableFrom(x) && x != validator_type)
                .ToList();


            foreach (var sub_validator_type in sub_validator_types)
            {
                Farm farm = Activator.CreateInstance(sub_validator_type) as Farm;

                if (farm != null)
                {
                    if (farm.Enabled)
                    {
                        farm.Init();
                        FarmList.Add(farm);
                    }
                }
            }
            return farmManager;
        }
        #endregion

        #region Methods
        public static Farm PlayerInFarmZone(IPlayer client)
        {
            Position pos = Position.Zero;
            if (client.GetPositionLocked(ref pos))
            {
                for(int i = 0; i < FarmList.Count; i++)
                {
                    Farm farm = FarmList[i];

                    for (int a = 0; a < farm.Harvest_Position.Count; a++)
                    {
                        if (farm.Harvest_Position[a].DistanceTo2D(pos) <= farm.Harvest_Range)
                            return farm;
                    }
                }
            }

            return null;
        }


        private void StartFarming(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            Farm farm = FarmList.Find(f => f.Harvest_Name == (string)args[0]);
            farm?.StartFarming(player);
        }
        #endregion
    }

    public abstract class Farm
    {
        #region Properties
        public bool Debug { get; set; }

        /*                  Harvest                     */
        [JsonIgnore]
        public Blips Harvest_Blip { get; set; }

        public string Harvest_Name { get; set; }
        public byte Harvest_BlipSprite { get; set; }
        public List<Vector3> Harvest_Position { get; set; } = new List<Vector3>();
        public Vector3 Harvest_BlipPosition { get; set; }
        public float Harvest_Range { get; set; } = 15f;
        public int Harvest_Time { get; set; } = 10000; // time to ms between to resource take
        public ItemID Harvest_ItemNeeded { get; set; }

        /*                  Process                     */
        [JsonIgnore]
        public Blips Process_Blip { get; set; }
        [JsonIgnore]
        public Ped Process_Ped { get; set; }

        public string Process_Name { get; set; }
        public int Process_BlipSprite { get; set; }
        public Location Process_PosRot { get; set; }
        public PedModel Process_PedHash { get; set; }
        public int Process_Time { get; set; } = 25000; // time to ms between to resource take
        public byte Process_QuantityNeeded { get; set; } = 2;

        /*              Double  Process                 */
        public string DoubleProcess_Name { get; set; }
        public int DoubleProcess_BlipSprite { get; set; }
        [JsonIgnore]
        public Blips DoubleProcess_Blip { get; set; }
        public Location DoubleProcess_PosRot { get; set; }
        [JsonIgnore]
        public Ped DoubleProcess_Ped { get; set; }
        public PedModel DoubleProcess_PedHash { get; set; }
        public int DoubleProcess_Time { get; set; } = 25000; // time to ms between to resource take

        /*                  Selling                     */
        public string Selling_Name { get; set; }
        public Blips Selling_Blip { get; set; }
        public int Selling_BlipSprite { get; set; }
        [JsonIgnore]
        public Ped Selling_Ped { get; set; }
        [JsonIgnore]
        public Location Selling_PosRot { get; set; }
        public PedModel Selling_PedHash { get; set; }
        public int Selling_Time { get; set; } = 1000; // time to ms between to resource take

        public BlipColor BlipColor { get; set; }
        public ItemID ItemIDBrute { get; set; }
        public ItemID ItemIDBrute2 { get; set; }
        public ItemID ItemIDProcess { get; set; }
        public int ItemPrice { get; set; }

        public bool Enabled { get; set; } = true;

        #region Timers
        public ConcurrentDictionary<IPlayer, System.Timers.Timer> FarmTimers = new ConcurrentDictionary<IPlayer, System.Timers.Timer>();
        public ConcurrentDictionary<IPlayer, System.Timers.Timer> ProcessTimers = new ConcurrentDictionary<IPlayer, System.Timers.Timer>();
        public ConcurrentDictionary<IPlayer, System.Timers.Timer> DoubleProcessTimers = new ConcurrentDictionary<IPlayer, System.Timers.Timer>();
        #endregion

        #endregion

        #region Init
        public virtual void Init()
        {
            #region Harvest          
            if (Harvest_Position.Count > 0)
            {
                if (Debug)
                {
                    foreach (Vector3 position in Harvest_Position)
                        Marker.CreateMarker(MarkerType.VerticalCylinder, position - new Vector3(0.0f, 0.0f, Harvest_Range), new Vector3(0, 0, Harvest_Range));
                }
            }

            if (Harvest_BlipPosition != new Vector3(0, 0, 0))
                Harvest_Blip = BlipsManager.CreateBlip(Harvest_Name, Harvest_BlipPosition, BlipColor, Harvest_BlipSprite);
            #endregion

            #region Process
            if (Process_PosRot != null)
            {
                Process_Blip = BlipsManager.CreateBlip(Process_Name, Process_PosRot.Pos, (byte)BlipColor, Process_BlipSprite);

                Process_Ped = Ped.CreateNPC(Process_PedHash, Process_PosRot.Pos, (int)Process_PosRot.Rot.Z);
                Process_Ped.NpcInteractCallBack += (IPlayer client, Ped npc) => { StartProcessing(client); return; };
            }

            if (DoubleProcess_PosRot != null)
            {
                DoubleProcess_Blip = BlipsManager.CreateBlip(DoubleProcess_Name, DoubleProcess_PosRot.Pos, (byte)BlipColor, DoubleProcess_BlipSprite);

                DoubleProcess_Ped = Ped.CreateNPC(DoubleProcess_PedHash, DoubleProcess_PosRot.Pos, (int)DoubleProcess_PosRot.Rot.Z);
                DoubleProcess_Ped.NpcInteractCallBack += ((IPlayer client, Ped npc) => { StartDoubleProcessing(client); });
            }

            #endregion

            #region Selling
            if (Selling_PosRot != null)
            {
                Selling_Blip = BlipsManager.CreateBlip(Selling_Name, Selling_PosRot.Pos, (int)BlipColor, Selling_BlipSprite);

                Selling_Ped = Ped.CreateNPC(Selling_PedHash, Selling_PosRot.Pos, (int)Selling_PosRot.Rot.Z);
                Selling_Ped.NpcInteractCallBack += (IPlayer client, Ped npc) => { StartSelling(client); return; };
            }
            #endregion
        }
        #endregion

        #region Event handlers
        public virtual void OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            return;
        }

        public virtual void OnPlayerExitColshape(IColshape colshape, IPlayer player)
        {
            PlayerHandler ph = player.GetPlayerHandler();

            if (ph != null)
                ph.IsOnProgress = false;
        }
        #endregion

        #region Methods
        public virtual void StartFarming(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);

            if (item == null)
                return;
            else if (client.IsInVehicle)
            {
                client.DisplaySubtitle($"~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                return;
            }
            else if (player.InventoryIsFull(item.weight))
            {
                client.SendNotificationError("Votre inventaire est déjà plein.");
                return;
            }

            client.DisplaySubtitle("Vous commencez à ramasser de(s) ~r~" + item.name, 5000);
            player.IsOnProgress = true;
            bool exit = false;
            int i = 0;

            FarmTimers[client] = Utils.Utils.SetInterval(() =>
            {
                if (!client.Exists)
                    return;

                if (exit)
                {
                    if (FarmTimers[client] != null)
                    {
                        FarmTimers[client].Close();
                        FarmTimers[client] = null;
                    }
                    return;
                }

                if (client.IsInVehicle)
                {
                    client.DisplaySubtitle($"~r~Récolte interrompu: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);

                    player.IsOnProgress = false;
                    player.UpdateFull();
                    exit = true;
                }
                else if (!IsInFarmingZone(client))
                {
                    client.DisplaySubtitle($"~r~Récolte interrompu: ~s~Vous devez rester dans la zone.", 5000);

                    player.IsOnProgress = false;
                    player.UpdateFull();
                    exit = true;
                }

                if (!player.AddItem(item, 1) || exit)
                {
                    exit = true;
                    client.DisplaySubtitle($"Récolte terminée: Vous avez ramassé ~r~ {i} {item.name}", 30000);
                    player.IsOnProgress = false;

                    player.UpdateFull();
                }
                else
                {
                    i++;
                    client.DisplaySubtitle($"~r~Récolte en cours: ~s~Vous venez de ramasser 1 {item.name}(s)", 5000);
                }
            }, Harvest_Time);
        }

        public virtual void StartProcessing(IPlayer sender)
        {
            if (sender == null || !sender.Exists || sender.IsInVehicle)
                return;

            PlayerHandler player = sender.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDProcess);

            if (!player.HasItemID(ItemIDBrute))
            {
                sender.DisplaySubtitle("~r~ERREUR ~s~Vous n'avez rien à traiter", 5000);
                return;
            }

            MenuManager.CloseMenu(sender);

            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);

            bool exit = false;
            int i = 0;
            ProcessTimers[sender] = Utils.Utils.SetInterval(() =>
            {
                if (!sender.Exists)
                    return;

                if (player.CountItem(ItemIDBrute) < Process_QuantityNeeded || exit)
                {
                    if (ProcessTimers[sender] != null)
                    {
                        ProcessTimers[sender].Stop();
                        ProcessTimers[sender] = null;
                    }
                }

                if (sender.IsInVehicle)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous ne pouvez pas traiter depuis le véhicule.", 5000);
                    exit = true;
                }
                else if (sender.Position.Distance(Process_Ped.Position) > 10f)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    exit = true;
                }

                if (exit)
                {
                    player.IsOnProgress = false;
                    return;
                }

                if (player.DeleteAllItem(ItemIDBrute, Process_QuantityNeeded))
                {
                    sender.DisplaySubtitle($"~r~Traitement en cours: ~s~+1 {_itemTraite.name}", 5000);
                    player.AddItem(_itemTraite, 1);
                    i++;
                }
                else
                {
                    sender.DisplaySubtitle($"Traitement terminé: Vous avez traité ~r~ {i} {_itemTraite.name}(s)", 15000);
                    player.UpdateFull();
                    player.IsOnProgress = false;
                    return;
                }
            }, Process_Time);

        }

        public virtual void StartSelling(IPlayer sender)
        {
            if (sender == null || !sender.Exists || sender.IsInVehicle)
                return;

            PlayerHandler player = sender.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            Item _itemBuy = Inventory.Inventory.ItemByID(ItemIDProcess);

            if (!player.HasItemID(ItemIDProcess))
            {
                sender.DisplaySubtitle("~r~ERREUR ~s~Vous n'avez rien à vendre", 5000);
                return;
            }

            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

            MenuManager.CloseMenu(sender);
            int itemcount = player.CountItem(_itemBuy);

            sender.EmitLocked("LaunchProgressBar", Selling_Time * itemcount);

            Utils.Utils.Delay(Selling_Time * itemcount, () =>
            {
                if (!sender.Exists)
                    return;

                if (player.DeleteAllItem(ItemIDProcess, itemcount))
                {
                    double gettaxe = Economy.Economy.CalculPriceTaxe((ItemPrice * itemcount), GameMode.Instance.Economy.Taxe_Exportation);
                    player.AddMoney((ItemPrice * itemcount) - gettaxe);
                    GameMode.Instance.Economy.CaissePublique += gettaxe;
                    sender.DisplaySubtitle($"~r~{itemcount} ~w~{_itemBuy.name}(s) $~r~{(ItemPrice * itemcount) - gettaxe} ~w~taxe:$~r~{gettaxe}.", 15000);
                }
                else
                    sender.SendNotificationError("Inconnu.");

                player.IsOnProgress = false;
                player.UpdateFull();
            });
        }

        public virtual void StartDoubleProcessing(IPlayer sender)
        {
            if (sender == null || !sender.Exists || sender.IsInVehicle)
                return;

            PlayerHandler player = sender.GetPlayerHandler();

            if (player == null || player.IsOnProgress)
                return;

            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemNoTraite2 = Inventory.Inventory.ItemByID(ItemIDBrute2);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDProcess);

            if (!player.HasItemID(ItemIDBrute))
            {
                player.Client.SendNotificationError($"Vous n'avez pas de {_itemNoTraite.name}");
                return;
            }
            else if (!player.HasItemID(ItemIDBrute2))
            {
                player.Client.SendNotificationError($"Vous n'avez pas de {_itemNoTraite2.name}");
                return;
            }

            MenuManager.CloseMenu(sender);

            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s) ~w~& ~r~{_itemNoTraite2.name}(s)", 5000);
            bool exit = false;
            var i = 0;

            DoubleProcessTimers[sender] = Utils.Utils.SetInterval(() =>
            {
                if (!sender.Exists)
                    return;

                if (sender.IsInVehicle)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous ne pouvez pas traiter depuis le véhicule.", 5000);
                    exit = true;
                }
                else if (sender.Position.Distance(DoubleProcess_Ped.Position) > 15f)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    exit = true;
                }

                if (exit)
                {
                    if (DoubleProcessTimers[sender] != null)
                    {
                        DoubleProcessTimers[sender].Stop();
                        DoubleProcessTimers[sender] = null;
                    }
                    player.UpdateFull();
                    player.IsOnProgress = false;
                    return;
                }

                sender.DisplaySubtitle($"~r~Traitement en cours: ~s~+1 {_itemTraite.name}", 5000);

                if (player.DeleteAllItem(ItemIDBrute, 1) && player.DeleteAllItem(ItemIDBrute2, 1))
                {
                    sender.DisplaySubtitle($"~r~Traitement en cours: ~s~+1 {_itemTraite.name}", 5000);
                    player.AddItem(_itemTraite, 1);
                    i++;
                }
                else
                {
                    sender.DisplaySubtitle($"Traitement terminé: Vous avez traité ~r~ {i} {_itemTraite.name}(s)", 15000);
                    exit = true;
                    player.UpdateFull();
                    player.IsOnProgress = false;
                    if (DoubleProcessTimers[sender] != null)
                    {
                        DoubleProcessTimers[sender].Stop();
                        DoubleProcessTimers[sender] = null;
                    }
                    return;
                }
            }, DoubleProcess_Time);
        }

        public bool IsInFarmingZone(IPlayer player)
        {
            var position = player.Position;

            foreach (Vector3 pos in Harvest_Position)
            {
                if (position.Distance(pos) < Harvest_Range) 
                    return true;
            }
            return false;
        }
        #endregion
    }
}
