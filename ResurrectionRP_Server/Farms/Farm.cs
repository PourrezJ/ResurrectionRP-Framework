using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Numerics;
using System.Linq;
using System.Drawing;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Models;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Blips;
using AltV.Net;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities;

namespace ResurrectionRP_Server.Farms
{
    public class FarmManager
    {
        public static List<Farm> FarmList = new List<Farm>();

        public static async Task<FarmManager> InitAll()
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
                        await farm.Init();
                        FarmList.Add(farm);
                    }
                }
            }
            return farmManager;
        }

        public static async Task<Farm> PlayerInFarmZone(IPlayer client)
        {
            foreach(Farm farm in FarmList)
            {
                foreach(Vector3 farmPos in farm.Harvest_Position)
                {
                    if (farmPos.DistanceTo2D(await client.GetPositionAsync()) <= farm.Harvest_Range)
                        return farm;
                }
            }
            return null;
        }

        private async Task StartFarming(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            Farm farm = FarmList.Find(f => f.Harvest_Name == (string)args[0]);
            await farm?.StartFarming(player);
        }
    }

    public abstract class Farm
    {
        #region Variable
        public bool Debug { get; set; }

        /*                  Harvest                     */
        [JsonIgnore]
        public IColShape Harvest_ColShape { get; set; }
        [JsonIgnore]
        public Entities.Blips.Blips Harvest_Blip { get; set; }

        public string Harvest_Name { get; set; }
        public byte Harvest_BlipSprite { get; set; }
        public List<Vector3> Harvest_Position { get; set; } = new List<Vector3>();
        public Vector3 Harvest_BlipPosition { get; set; }
        public float Harvest_Range { get; set; } = 15f;
        public int Harvest_Time { get; set; } = 10000; // time to ms between to resource take
        public ItemID Harvest_ItemNeeded { get; set; }

        /*                  Process                     */
        [JsonIgnore]
        public Entities.Blips.Blips Process_Blip { get; set; }
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
        public Entities.Blips.Blips DoubleProcess_Blip { get; set; }
        public Location DoubleProcess_PosRot { get; set; }
        [JsonIgnore]
        public Ped DoubleProcess_Ped { get; set; }
        public PedModel DoubleProcess_PedHash { get; set; }
        public int DoubleProcess_Time { get; set; } = 25000; // time to ms between to resource take

        /*                  Selling                     */
        public string Selling_Name { get; set; }
        public Entities.Blips.Blips Selling_Blip { get; set; }
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

        #endregion

        #region Method

        public virtual Task Init()
        {
            #region Harvest          
            if (Harvest_Position.Count > 0)
            {
                foreach (Vector3 position in Harvest_Position)
                {
                    Harvest_ColShape = Alt.CreateColShapeCylinder(position, Harvest_Range, 4);

                    if (Debug)
                    {
                        Marker.CreateMarker(MarkerType.VerticalCylinder, position - new Vector3(0.0f, 0.0f, Harvest_Range), new Vector3(0,0, Harvest_Range));
                    }
                }
            }

            if (Harvest_BlipPosition != new Vector3(0, 0, 0))
                Harvest_Blip = BlipsManager.CreateBlip(Harvest_Name, Harvest_BlipPosition, BlipColor, Harvest_BlipSprite);


            #endregion

            #region Process
            if (Process_PosRot != null)
            {
                Process_Blip = BlipsManager.CreateBlip(Process_Name, Process_PosRot.Pos, (byte)BlipColor, Process_BlipSprite);

                Process_Ped = Ped.CreateNPC(Process_PedHash, Streamer.Data.PedType.Human, Process_PosRot.Pos, (int)Process_PosRot.Rot.Z);
                Process_Ped.NpcInteractCallBack += (async (IPlayer client, Ped npc) => { await StartProcessing(client); });
            }

            if (DoubleProcess_PosRot != null)
            {
                DoubleProcess_Blip = BlipsManager.CreateBlip(DoubleProcess_Name, DoubleProcess_PosRot.Pos, (byte)BlipColor, DoubleProcess_BlipSprite);

                DoubleProcess_Ped = Ped.CreateNPC(DoubleProcess_PedHash, Streamer.Data.PedType.Human, DoubleProcess_PosRot.Pos, (int)DoubleProcess_PosRot.Rot.Z);
                DoubleProcess_Ped.NpcInteractCallBack += (async (IPlayer client, Ped npc) => { await StartDoubleProcessing(client); });
            }

            #endregion

            #region Selling
            if (Selling_PosRot != null)
            {
                Selling_Blip = BlipsManager.CreateBlip(Selling_Name, Selling_PosRot.Pos, (int)BlipColor, Selling_BlipSprite);

                Selling_Ped = Ped.CreateNPC(Selling_PedHash, Streamer.Data.PedType.Human, Selling_PosRot.Pos, (int)Selling_PosRot.Rot.Z);
                Selling_Ped.NpcInteractCallBack += (async (IPlayer client, Ped npc) => { await StartSelling(client); });
            }
            #endregion

            return Task.CompletedTask;
        }

        public virtual async Task StartFarming(IPlayer client)
        {
            if (!client.Exists)
                return;
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.IsOnProgress || await client.IsInVehicleAsync())
                return;


            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            if (item == null)
                return;

            if (await client.IsInVehicleAsync())
            {
                client.DisplaySubtitle($"~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                return;
            }

            if (player.InventoryIsFull(item.weight))
            {
                client.SendNotificationError("Votre inventaire est déjà plein.");
                return;
            }

            client.DisplaySubtitle("Vous commencez à ramasser de(s) ~r~" + item.name, 5000);
            player.IsOnProgress = true;
            bool _exit = false;
            int i = 0;
            while (!_exit)
            {
                if (!client.Exists)
                    return;

                await Task.Delay(Harvest_Time);
                if (await client.IsInVehicleAsync())
                {
                    client.DisplaySubtitle($"~r~Récolte interrompu: ~s~Vous ne pouvez pas récolter depuis le véhicule.", 5000);
                    _exit = true;
                }
                else if (!await IsInFarmingZone(client))
                {
                    client.DisplaySubtitle($"~r~Récolte interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    _exit = true;
                }

                if (!await player.AddItem(item, 1) || _exit)
                {
                    _exit = true;
                    client.DisplaySubtitle($"Récolte terminée: Vous avez ramassé ~r~ {i} {item.name}", 30000);
                    player.IsOnProgress = false;
                }
                else
                {
                    i++;
                    client.DisplaySubtitle($"~r~Récolte en cours: ~s~Vous venez de ramasser 1 {item.name}(s)", 5000);
                }
            }

            player.IsOnProgress = false;
            await player.Update();
        }

        public virtual async Task StartProcessing(IPlayer sender)
        {
            PlayerHandler player = sender.GetPlayerHandler();
            if (player.IsOnProgress || await sender.IsInVehicleAsync()) return;

            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDProcess);

            if (!player.HasItemID(ItemIDBrute))
            {
                sender.DisplaySubtitle("~r~ERREUR ~s~Vous n'avez rien à traiter", 5000);
                return;
            }

            await MenuManager.CloseMenu(sender);

            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s)", 5000);
            bool _exit = false;
            var i = 0;
            while (!_exit)
            {
                if (!sender.Exists)
                    return;

                if (player.CountItem(ItemIDBrute) >= Process_QuantityNeeded)
                    await Task.Delay(Process_Time);


                if (await sender.IsInVehicleAsync())
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous ne pouvez pas traiter depuis le véhicule.", 5000);
                    _exit = true;
                }
                else if ((await sender.GetPositionAsync()).Distance(Process_Ped.Position) > 10f)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    _exit = true;
                }

                if (_exit)
                {
                    player.IsOnProgress = false;
                    return;
                }

                if (player.DeleteAllItem(ItemIDBrute, Process_QuantityNeeded))
                {
                    sender.DisplaySubtitle($"~r~Traitement en cours: ~s~+1 {_itemTraite.name}", 5000);
                    await player.AddItem(_itemTraite, 1);
                    i++;
                }
                else
                {
                    sender.DisplaySubtitle($"Traitement terminé: Vous avez traité ~r~ {i} {_itemTraite.name}(s)", 15000);
                    await player.Update();
                    player.IsOnProgress = false;
                    return;
                }
            }
        }

        public virtual async Task StartSelling(IPlayer sender)
        {
            PlayerHandler player = sender.GetPlayerHandler();
            if (player.IsOnProgress || await sender.IsInVehicleAsync()) return;
            Item _itemBuy = Inventory.Inventory.ItemByID(ItemIDProcess);

            if (!player.HasItemID(ItemIDProcess))
            {
                sender.DisplaySubtitle("~r~ERREUR ~s~Vous n'avez rien à vendre", 5000);
                return;
            }
            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à vendre vos ~r~{_itemBuy.name}(s)", 5000);

            await MenuManager.CloseMenu(sender);
            int itemcount = player.CountItem(_itemBuy);

            await sender.EmitAsync("LaunchProgressBar", Selling_Time * itemcount);
            await Task.Delay(Selling_Time * itemcount);

            if (!sender.Exists)
                return;

            if (player.DeleteAllItem(ItemIDProcess, itemcount))
            {
                /* ECONOMY a besoin d'être implanté
                double gettaxe = Economy.CalculPriceTaxe((ItemPrice * itemcount), GameMode.Instance.Economy.Taxe_Exportation);
                await player.AddMoney((ItemPrice * itemcount) - gettaxe);
                GameMode.Instance.Economy.CaissePublique += gettaxe;
                await sender.DisplaySubtitle($"~r~{itemcount} ~w~{_itemBuy.name}(s) $~r~{(ItemPrice * itemcount) - gettaxe} ~w~taxe:$~r~{gettaxe}.", 15000);*/
            }
            else
                sender.SendNotificationError("Inconnu.");

            player.IsOnProgress = false;
            await player.Update();
        }

        public virtual async Task StartDoubleProcessing(IPlayer sender)
        {
            PlayerHandler player = sender.GetPlayerHandler();
            if (player.IsOnProgress || await sender.IsInVehicleAsync()) return;

            Item _itemNoTraite = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item _itemNoTraite2 = Inventory.Inventory.ItemByID(ItemIDBrute2);
            Item _itemTraite = Inventory.Inventory.ItemByID(ItemIDProcess);

            if (!player.HasItemID(ItemIDBrute))
            {
                player.Client.SendNotificationError($"Vous n'avez pas de {_itemNoTraite.name}");
                return;
            }

            if (!player.HasItemID(ItemIDBrute2))
            {
                player.Client.SendNotificationError($"Vous n'avez pas de {_itemNoTraite2.name}");
                return;
            }

            await MenuManager.CloseMenu(sender);

            player.IsOnProgress = true;
            sender.DisplaySubtitle($"Vous commencez à traiter vos ~r~{_itemNoTraite.name}(s) ~w~& ~r~{_itemNoTraite2.name}(s)", 5000);
            bool _exit = false;
            var i = 0;
            while (!_exit)
            {
                if (!sender.Exists)
                    return;

                if (player.CountItem(ItemIDBrute) >= 1 && player.CountItem(ItemIDBrute2) >= 1)
                    await Task.Delay(DoubleProcess_Time);


                if (!sender.Exists)
                    return;

                if (await sender.IsInVehicleAsync())
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous ne pouvez pas traiter depuis le véhicule.", 5000);
                    _exit = true;
                }
                else if ((await sender.GetPositionAsync()).Distance(DoubleProcess_Ped.Position) > 15f)
                {
                    sender.DisplaySubtitle($"~r~Traitement interrompu: ~s~Vous devez rester dans la zone.", 5000);
                    _exit = true;
                }

                if (_exit)
                {
                    player.IsOnProgress = false;
                    return;
                }

                sender.DisplaySubtitle($"~r~Traitement en cours: ~s~+1 {_itemTraite.name}", 5000);

                if (player.DeleteAllItem(ItemIDBrute, 1) && player.DeleteAllItem(ItemIDBrute2, 1))
                {
                    sender.DisplaySubtitle($"~r~Traitement en cours: ~s~+1 {_itemTraite.name}", 5000);
                    await player.AddItem(_itemTraite, 1);
                    i++;
                }
                else
                {
                    sender.DisplaySubtitle($"Traitement terminé: Vous avez traité ~r~ {i} {_itemTraite.name}(s)", 15000);
                    await player.Update();
                    player.IsOnProgress = false;
                    return;
                }
            }
        }

        public async Task<bool> IsInFarmingZone(IPlayer player)
        {
            foreach (Vector3 pos in Harvest_Position)
            {
                if ((await (player.GetPositionAsync())).Distance(pos) < Harvest_Range) return true;
            }
            return false;
        }

        public virtual Task OnPlayerColshape(IColShape colShape, IPlayer client)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnExitColshape(IColShape colShape, IPlayer player)
        {
            var ph = player.GetPlayerHandler();
            if (ph != null)
                ph.IsOnProgress = false;

            return Task.CompletedTask;
        }
        #endregion
    }
}
