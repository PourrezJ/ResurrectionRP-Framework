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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace ResurrectionRP_Server.Farms
{
    class Cuivre : Farm
    {
        private static int UsureOutil = 1;

        public Cuivre()
        {
            NewFarm = true;
            Harvest_Name = "Mine de cuivre";
            Process_Name = "Fonderie de cuivre";
            Selling_Name = "Revendeur de cuivre";

            Harvest_BlipSprite = 85;
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
                FarmPoints.Add( new InteractionPoint(this, position, 0,  new List<Item> { Inventory.Inventory.ItemByID(ItemID.Pioche), Inventory.Inventory.ItemByID(ItemID.MarteauPiqueur) }, InteractionPointTypes.Farm, "miner", 0))
            );
            Harvest_Range = 100f;

            new List<Vector3>
            {
                new Vector3(1092.4088f, -1996.5626f, 29.616821f),
                new Vector3(1096.5231f, -1994.2946f, 29.364136f),
                new Vector3(1089.956f, -1991.011f, 28.976562f)
            }.ForEach((position) =>
                DoubleProcessPoints.Add(new InteractionPoint(this, position, 90,  Inventory.Inventory.ItemByID(ItemID.Marteau), InteractionPointTypes.DoubleProcess, "forger", 0.0f))
            );

            new List<Vector3>
            {
                new Vector3(1086f, -2001.493f, 31.382f)
            }.ForEach((position) =>
                ProcessPoints.Add(new InteractionPoint(this, new Vector3(1086f, -2001.493f, 31.382f), -2.3f,  Inventory.Inventory.ItemByID(ItemID.Marteau), InteractionPointTypes.Process, "fondre", 0.0f))
            );
            
            Selling_PosRot = new Location(new Vector3(605.719f, -3073.165f, 8.069f), new Vector3(0, 0, -11.52882f));
            Selling_PedHash = AltV.Net.Enums.PedModel.Cntrybar01SMM;

            BlipColor = Entities.Blips.BlipColor.Complexion;


            Process_Blip = BlipsManager.CreateBlip(Process_Name, new Vector3(1086f, -2001.493f, 31.382f), (byte)BlipColor, 499);
            Process_QuantityNeeded = 2;
            Process_Time = 5000;

            DoubleProcess_Time = 5000;

            ItemIDBrute = ItemID.MineraiCuivre;
            ItemIDProcess = ItemID.Cuivre;
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
                _item.SetPickaxeAnimation(client, (int)(Harvest_Time / _item.Speed));
                client.DisplayHelp($"Durabilité: {_item.Health - UsureOutil}\nMinerais récoltés: {_item.MiningRate}\nVitesse: {_item.Speed}" , 5000);
                _item.Health -= UsureOutil;
            }
            else if (_itembis != null)
            {
                _itembis.SetJackHammerAnimation(client, (int)(Harvest_Time / _itembis.Speed));
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
                WorkingPlayers.TryAdd(client.Id, client);
                PlayerHandler player = client.GetPlayerHandler();
                Item item = Inventory.Inventory.ItemByID(ItemID.CuivreFondu);
                Pickaxe _item = (Pickaxe)(player.OutfitInventory.HasItemEquip(ItemID.Marteau)?.Item);

                if (player.BagInventory.CountItem(ItemID.MineraiCuivre) < Process_QuantityNeeded* _item.MiningRate && player.PocketInventory.CountItem(ItemID.MineraiCuivre) < Process_QuantityNeeded* _item.MiningRate)
                {
                    client.DisplayHelp("Vous n'avez plus de cuivre sur vous à fondre!");
                    return;
                }

                client.TaskAdvancedPlayAnimation("anim@heists@load_box", "load_box_1_box_a", new Vector3(1086f, -2001.493f, 31.382f), new Vector3(0, 0, 0), 1, 1, 15000, 1, 5000);
                client.Freeze(true);
                Utils.Utils.Delay((int)(Process_Time / _item.Speed), () =>
                {

                    if (!client.Exists)
                        return;
                    if (player.DeleteAllItem(ItemIDBrute, Process_QuantityNeeded * _item.MiningRate))
                    {
                        client.Freeze(false);
                        client.DisplaySubtitle($"Vous avez fondu ~r~ {_item.MiningRate} {item.name}", 5000);
                        WorkingPlayers.TryRemove(client.Id, out IPlayer voided);
                        player.AddItem(item, _item.MiningRate);
                        client.StopAnimation();
                        player.UpdateFull();
                    }
                });
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

                DoubleProcessPoints.ForEach((p) =>
                   {
                       if( p.Position.DistanceTo2D(client.Position.ConvertToVector3()) < 2)
                           client.TaskStartScenarioAtPosition("WORLD_HUMAN_HAMMERING", p.Position, p.Heading, Process_Time, false, false);
                   });

                WorkingPlayers.TryAdd(client.Id, client);
                Utils.Utils.Delay((int)(DoubleProcess_Time / _item.Speed), () =>
                { 
                    if (!client.Exists)
                        return;
                    if (player.DeleteAllItem(ItemID.CuivreFondu, _item.MiningRate))
                    {
                        WorkingPlayers.TryRemove(client.Id, out IPlayer pp);
                        client.DisplaySubtitle($"Vous avez forgé ~r~ {_item.MiningRate} {item.name}", 5000);
                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                        player.AddItem(item, _item.MiningRate);
                        client.StopAnimation();
                        player.UpdateFull();
                    }

                    ProcessTimers[client].Stop();
                    ProcessTimers[client].Close();
                    ProcessTimers.TryRemove(client, out _);
                });
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Copper Farm | StartDoubleProcess | " + ex.Data);
            }

        }
    }
    
}
