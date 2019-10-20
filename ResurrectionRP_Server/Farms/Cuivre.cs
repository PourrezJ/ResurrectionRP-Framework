using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Enums;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace ResurrectionRP_Server.Farms
{
    public enum InteractionPointTypes
    {
        Farm,
        Process,
        DoubleProcess,
        Sell
    }
    public class InteractionPoint
    {
        private Farm Farm;
        public Vector3 Position;
        public Item ReturnedItem;
        public float DoubleLuck = 0.0f; // Un randomint sera fait pour savoir si on veut 2 minerais
        public IColshape Colshape;

        public List<Item> ToolNeeded;
        public InteractionPointTypes Type;
        public string InteractionName;

        public InteractionPoint(Farm farm, Vector3 position, Vector3 rotation, Item returnedItem, List<Item> items, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            ReturnedItem = returnedItem;
            DoubleLuck = doubleLuck;
            ToolNeeded = items;
            Type = interactionPoint;
            InteractionName = interactionName;
            Farm = farm;
            Init();
        }
        public InteractionPoint(Farm farm, Vector3 position, Vector3 rotation,Item returnedItem, Item item, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            ReturnedItem = returnedItem;
            DoubleLuck = doubleLuck;
            ToolNeeded = new List<Item> { item };
            Type = interactionPoint;
            InteractionName = interactionName;
            Farm = farm;
            Init();
        }

        private void Init()
        {
            Colshape = ColshapeManager.CreateCylinderColshape(Position - new Vector3(0, 0, 1), 2, 3);
            Colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;
            Colshape.OnPlayerInteractInColshape += Colshape_OnPlayerInteractInColshape;

            Marker.CreateMarker(MarkerType.MarkerTypeHorizontalBars, Position + new Vector3(0, 0, 1), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(128, 255, 69, 0));
        }

        private void Colshape_OnPlayerInteractInColshape(IColshape colshape, AltV.Net.Elements.Entities.IPlayer client)
        {
            if (Farm.FarmTimers.ContainsKey(client) || Farm.ProcessTimers.ContainsKey(client) || Farm.DoubleProcessTimers.ContainsKey(client))
                return;
            try
            {
                ToolNeeded.ForEach((_item) =>
                {
                    PlayerHandler _client = client.GetPlayerHandler();
                    Models.ItemStack item = _client.OutfitInventory.HasItemEquip(_item.id);

                    if (client.IsInVehicle)
                        client.DisplayHelp("Vous ne pouvez faire être ici avec un véhicule!", 5000);
                    else if (item == null && ToolNeeded.IndexOf(_item) != ToolNeeded.Count -1)
                        client.DisplayHelp($"Vous devez avoir un outil pour {InteractionName} !", 10000);
                    else if ((item != null && item.Item.type == "pickaxe" && (item.Item as Pickaxe).Health <= 0))
                    {
                        _client.OutfitInventory.Delete(item, 1);
                        client.DisplayHelp("Votre outil s'est cassé, vous êtes bon pour en racheter un !", 10000);
                    }
                    else if (item != null)
                    {
                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à " + InteractionName, 5000);

                        switch(Type)
                        {
                            case InteractionPointTypes.Farm:
                                Farm?.StartFarming(client);
                                break;
                            case InteractionPointTypes.DoubleProcess:
                                Farm?.StartDoubleProcessing(client);
                                break;
                            case InteractionPointTypes.Process:
                                Farm?.StartProcessing(client);
                                break;
                            case InteractionPointTypes.Sell:
                                Farm?.StartSelling(client);
                                break;
                            default:
                                Alt.Server.LogError("Problem, an unknwn Interactin type in InteractionPoints! ");
                                break;
                        }
                        return;
                    }
                });

            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Cuivre farming interact colshape: " + ex.Data);
            }
        }

        private void Colshape_OnPlayerEnterColshape(IColshape colshape, AltV.Net.Elements.Entities.IPlayer client)
        {

            try
            {
                ToolNeeded.ForEach((_item) =>
                {
                    PlayerHandler _client = client.GetPlayerHandler();
                    Inventory.Inventory inventory = _client.HasItemInAnyInventory(_item.id);
                    Models.ItemStack item = _client.OutfitInventory.HasItemEquip(_item.id);

                    if (client.IsInVehicle)
                        client.DisplayHelp("Vous ne pouvez faire être ici avec un véhicule!", 5000);
                    else if (inventory != null && item == null)
                        client.DisplayHelp("Vous devez équiper votre outil pour commencer!", 5000);
                    else if (item == null && ToolNeeded.IndexOf(_item) != ToolNeeded.Count -1)
                        client.DisplayHelp($"Vous devez avoir un outil pour {InteractionName} !", 10000);
                    else if ((item != null && item.Item.type == "pickaxe" && (item.Item as Pickaxe).Health <= 0))
                    {
                        _client.OutfitInventory.Delete(item, 1);
                        client.DisplayHelp("Votre outil s'est cassé, vous êtes bon pour en racheter un !", 10000);
                    }
                    else if(item != null)
                    {
                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à " + InteractionName, 5000);
                        return;
                    }
                });

            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Cuivre farming enter colshape: " + ex.Data);
            }

        }
    }

    class Cuivre : Farm
    {
        private static int UsureOutil = 1;
        [JsonIgnore]
        private IColshape Process_Colshape { get; set; } = null;
        [JsonIgnore]
        private Marker Process_Marker { get; set; } = null;
        [JsonIgnore]
        public List<InteractionPoint> FarmPoints { get; set; } = new List<InteractionPoint>();
        [JsonIgnore]
        public List<InteractionPoint> ForgePoints { get; set; } = new List<InteractionPoint>();
        [JsonIgnore]
        public List<InteractionPoint> FonduPoints { get; set; } = new List<InteractionPoint>();
        public Cuivre()
        {
            NewFarm = true;
            Harvest_Name = "Mine de cuivre";
            Process_Name = "Fonderie de cuivre";
            Selling_Name = "Revendeur de cuivre";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_Time = 5000;
            Harvest_BlipPosition = new Vector3(2964.211f, 2817.4153f, 42.759766f);
            Harvest_Position.Add(new Vector3(2938.51f, 2770.522f, 38.47805f));
            Harvest_Position.Add(new Vector3(2951.971f, 2769.79f, 38.42247f));
            Harvest_Position.Add(new Vector3(2970.657f, 2776.359f, 37.7042f));
            Harvest_Position.Add(new Vector3(2943.139f, 2819.934f, 42.5276f));
            Harvest_Position.Add(new Vector3(2997.679f, 2758.425f, 42.34139f));
            Harvest_Position.Add(new Vector3(2946.29f, 2733.697f, 44.75487f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add( new InteractionPoint(this, position, new Vector3(), Inventory.Inventory.ItemByID(ItemID.MineraiCuivre), new List<Item> { Inventory.Inventory.ItemByID(ItemID.Pioche), Inventory.Inventory.ItemByID(ItemID.MarteauPiqueur) }, InteractionPointTypes.Farm, "miner", 0))
            );
            Harvest_Range = 100f;

            new List<Vector3>
            {
                new Vector3(1092.4088f, -1996.5626f, 29.616821f),
                new Vector3(1096.5231f, -1994.2946f, 29.364136f),
                new Vector3(1089.956f, -1991.011f, 28.976562f)
            }.ForEach((position) =>
                ForgePoints.Add(new InteractionPoint(this, position, new Vector3(0, 0, -2.3f), Inventory.Inventory.ItemByID(ItemID.Cuivre), Inventory.Inventory.ItemByID(ItemID.Marteau), InteractionPointTypes.DoubleProcess, "forger", 0.0f))
            );

            new List<Vector3>
            {
                new Vector3(1086f, -2001.493f, 31.382f)
            }.ForEach((position) =>
                FonduPoints.Add(new InteractionPoint(this, new Vector3(1086f, -2001.493f, 31.382f), new Vector3(0, 0, -2.3f), Inventory.Inventory.ItemByID(ItemID.CuivreFondu), Inventory.Inventory.ItemByID(ItemID.Marteau), InteractionPointTypes.Process, "fondre", 0.0f))
            );
            
            Selling_PosRot = new Location(new Vector3(-788.2813f, 5868.633f, 6.3809814f), new Vector3(0, 0, -11.52882f));
            Selling_PedHash = AltV.Net.Enums.PedModel.Cntrybar01SMM;

            BlipColor = Entities.Blips.BlipColor.Complexion;


            Process_Blip = BlipsManager.CreateBlip(Process_Name, new Vector3(1086f, -2001.493f, 31.382f), (byte)BlipColor, Process_BlipSprite);
            Process_QuantityNeeded = 2;
            Process_Time = 5000;

            DoubleProcess_Time = 5;

            ItemIDBrute = ItemID.MineraiCuivre;
            ItemIDProcess = ItemID.CuivreFondu;
            ItemPrice = 92;
        }

        public override void StartFarming(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"Player {client.GetPlayerHandler()?.PID} is now farming at Copper");

            PlayerHandler player = client.GetPlayerHandler();
            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            Pickaxe _item = (Pickaxe)(player.OutfitInventory.HasItemEquip(ItemID.Pioche)?.Item);
            Pickaxe _itembis = (Pickaxe)(player.OutfitInventory.HasItemEquip(ItemID.MarteauPiqueur)?.Item);
            Pickaxe usedItem = (_item == null) ? _itembis : _item;



            if (player == null || player.IsOnProgress)
                return;
            if (player.InventoryIsFull(item.weight))
            {
                client.DisplayHelp("Votre inventaire est déjà plein.", 10000);
                return;
            }
            if (_item == null && _itembis == null)
                return;
            if (_item != null)
            {
                client.PlayAnimation("melee@large_wpn@streamed_core", "ground_attack_on_spot", 8, -1, (int)(Harvest_Time / _item.Speed), (AnimationFlags)49);
                client.DisplayHelp($"Durabilité: {_item.Health - UsureOutil}\nMinerais récoltés: 1\nVitesse: {_item.Speed}" , 5000);
                _item.Health -= UsureOutil;
            }
            else if (_itembis != null)
            {
                player.OutfitInventory.prop.SetAttachToEntity(client, "PH_R_Hand", new Vector3(0.1f, -0.1f, -0.02f), new Vector3(0, 0, 170));
                client.PlayAnimation("AMB@WORLD_HUMAN_CONST_DRILL@MALE@DRILL@BASE", "base", 8, -1, (int)(Harvest_Time / _itembis.Speed), (AnimationFlags)49);
                client.DisplayHelp($"Durabilité: {_itembis.Health - UsureOutil}\nMinerais récoltés: 5\nVitesse: {_itembis.Speed}", 5000);
                _itembis.Health -= UsureOutil;
            }
            else
                Alt.Server.LogError("Cuivre - StartFarming - Can't farm no more item wtf");


            FarmTimers[client] = Utils.Utils.SetInterval(() =>
            {
                if (!client.Exists)
                    return;

                if (player.AddItem(item, usedItem.MiningRate))
                
                    client.DisplaySubtitle($"Vous avez miné ~r~ {usedItem.MiningRate} {item.name}", 5000);
                 else
                
                    client.DisplayHelp("Plus de place dans votre inventaire!");
                
                player.UpdateFull();
                FarmTimers[client].Stop();
                FarmTimers[client].Close();
                FarmTimers.TryRemove(client, out _);

            },(int) (Harvest_Time / usedItem.Speed));

        }

        public override void StartProcessing(IPlayer client)
        {
            try
            {
                PlayerHandler player = client.GetPlayerHandler();
                Item item = Inventory.Inventory.ItemByID(ItemIDProcess);
                Pickaxe _item = (Pickaxe)(player.OutfitInventory.HasItemEquip(ItemID.Marteau)?.Item);

                if(player.BagInventory.CountItem(ItemID.MineraiCuivre) < Process_QuantityNeeded* _item.MiningRate && player.PocketInventory.CountItem(ItemID.MineraiCuivre) < Process_QuantityNeeded* _item.MiningRate)
                {
                    client.DisplayHelp("Vous n'avez plus de cuivre sur vous à fondre!");
                    return;
                }

                client.TaskAdvancedPlayAnimation("anim@heists@load_box", "load_box_1_box_a", new Vector3(1086f, -2001.493f, 31.382f), new Vector3(0, 0, 0), 1, 1, 15000, 1, 5000);
                DoubleProcessTimers[client] = Utils.Utils.SetInterval(() =>
                {
                    if (!client.Exists)
                        return;
                    if (player.DeleteAllItem(ItemIDBrute, Process_QuantityNeeded * _item.MiningRate))
                    {
                        client.DisplaySubtitle($"Vous avez fondu ~r~ {_item.MiningRate} {item.name}", 5000);
                        player.AddItem(item, _item.MiningRate);
                        player.UpdateFull();
                    }

                    DoubleProcessTimers[client].Stop();
                    DoubleProcessTimers[client].Close();
                    DoubleProcessTimers.TryRemove(client, out _);
                }, (int)(DoubleProcess_Time / _item.Speed));
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Copper Farm | StartProcessing | " + ex.Data);
            }

        }

        public override void StartDoubleProcessing(IPlayer client)
        {
            try
            {
                PlayerHandler player = client.GetPlayerHandler();
                Item item = Inventory.Inventory.ItemByID(ItemID.Cuivre);
                Pickaxe _item = (Pickaxe)(player.OutfitInventory.HasItemEquip(ItemID.Marteau)?.Item);
                if (player.BagInventory.CountItem(ItemID.CuivreFondu) < _item.MiningRate && player.PocketInventory.CountItem(ItemID.CuivreFondu) < _item.MiningRate)
                {
                    client.DisplayHelp("Vous n'avez plus de cuivre sur vous à fondre!");
                    return;
                }
                client.TaskStartScenarioAtPosition("WORLD_HUMAN_HAMMERING", client.Position.ConvertToVector3(), 0, 5000, false, false);

                ProcessTimers[client] = Utils.Utils.SetInterval(() =>
                {
                    if (!client.Exists)
                        return;
                    if (player.DeleteAllItem(ItemID.CuivreFondu, _item.MiningRate))
                    {
                        client.DisplaySubtitle($"Vous avez forgé ~r~ {_item.MiningRate} {item.name}", 5000);
                        player.AddItem(item, _item.MiningRate);
                        player.UpdateFull();
                    }

                    ProcessTimers[client].Stop();
                    ProcessTimers[client].Close();
                    ProcessTimers.TryRemove(client, out _);
                }, (int)(Process_Time / _item.Speed));
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Copper Farm | StartDoubleProcess | " + ex.Data);
            }

        }
    }
    
}
