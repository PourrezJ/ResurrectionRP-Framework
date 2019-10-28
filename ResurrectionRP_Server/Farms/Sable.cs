using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Sable : Farm
    {
        private static int UsureOutil = 1;
        public Sable()
        {
            NewFarm = true;
            Harvest_Name = "Sable";
            Process_Name = "Traitement de verre";
            DoubleProcess_Name = "Stérilisation des bouteilles";
            Selling_Name = "Revendeur de bouteilles";

            Harvest_BlipSprite = 85;

            Harvest_BlipPosition = new Vector3(-788.2813f, 5868.633f, 6.380f);
            Harvest_Position.Add(new Vector3(-756.78f, 5876.49f, 8.8f));
            Harvest_Position.Add(new Vector3(-774.936f, 5875.556f, 7.791f));
            Harvest_Position.Add(new Vector3(-794.6f, 5874.446f, 6.134f));
            Harvest_Position.Add(new Vector3(-791.688f, 5864.644f, 5.551f));
            Harvest_Position.Add(new Vector3(-772.354f, 5867.396f, 8.084f));
            Harvest_Position.Add(new Vector3(-751.615f, 5868.162f, 9.774f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, Inventory.Inventory.ItemByID(ItemID.Pelle), InteractionPointTypes.Farm, "creuser"))
            );


            new List<Vector3>
            {
                new Vector3(2942.045f, 4626.106f, 48.721f),
                new Vector3(2937.887f, 4620.325f, 48.721f)
            }.ForEach((position) =>
                ProcessPoints.Add(new InteractionPoint(this, position, -2.3f, Inventory.Inventory.ItemByID(ItemID.Soufflet), InteractionPointTypes.Process, "souffler"))
            );

            new List<Vector3>
            {
                new Vector3(360.063f, 3405.598f , 36.404f),
                new Vector3(358.274f, 3409.92f, 36.404f),
                new Vector3(363.619f, 3403.789f, 36.404f),
            }.ForEach((position) =>
                DoubleProcessPoints.Add(new InteractionPoint(this, position, 90, InteractionPointTypes.DoubleProcess, "stériliser"))
            );

            ConcurrentDictionary<double, Item> eligiblelist = new ConcurrentDictionary<double, Item>();
            eligiblelist.TryAdd(98, Inventory.Inventory.ItemByID(ItemID.Bouteille));
            eligiblelist.TryAdd(410, Inventory.Inventory.ItemByID(ItemID.BouteilleTraite));

            new List<Vector3>
            {
                new Vector3(498.319f, -627.77f, 24.751f)
            }.ForEach((position) =>

               SellingPoints.Add(new InteractionPoint(this, position, 215, AltV.Net.Enums.PedModel.BoatStaff01F, eligiblelist, InteractionPointTypes.Sell, "vendre"))
           );


            Harvest_Range = 50f;

            BlipColor = Entities.Blips.BlipColor.Yellow;
            Process_Blip = BlipsManager.CreateBlip(Process_Name, new Vector3(2942.045f, 4626.106f, 48.721f), BlipColor, 499);
            DoubleProcess_Blip = BlipsManager.CreateBlip(DoubleProcess_Name, new Vector3(360.063f, 3405.598f, 36.404f), BlipColor, 499);
            Selling_Blip = BlipsManager.CreateBlip(Selling_Name, new Vector3(498.319f, -627.77f, 24.751f), BlipColor, 500);

            ItemIDBrute = ItemID.Sable;
            ItemIDProcess = ItemID.Bouteille;

            Process_QuantityNeeded = 2;

            Harvest_Time = 5000;
            Process_Time = 5000;
            DoubleProcess_Time = 5000;
        }



        public override void StartFarming(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;
            PlayerHandler player = client.GetPlayerHandler();
            Item item = Inventory.Inventory.ItemByID(ItemIDBrute);
            Tool _item = (Tool)(player.OutfitInventory.HasItemEquip(ItemID.Pelle)?.Item);



            if (player == null || player.IsOnProgress)
                return;
            if (player.InventoryIsFull(item.weight))
            {
                client.DisplayHelp("Votre inventaire est déjà plein.", 10000);
                return;
            }
            if (_item == null)
                return;

            int rate = client.GetPlayerHandler().HasItemID(ItemID.Seau) ? 5 : _item.MiningRate;
            if (_item != null)
            {
                client.DisplayHelp($"Durabilité: {_item.Health - UsureOutil}\nSable récoltés: {rate}\nVitesse: {_item.Speed}", 5000);
                _item.Health -= UsureOutil;
            }
            else
                Alt.Server.LogError("Sand - StartFarming - Can't farm no more item wtf");

            WorkingPlayers.TryAdd(client.Id, client);
            Utils.Utils.Delay((int)(Harvest_Time / _item.Speed), () =>
            {

                if (!client.Exists)
                    return;

                if (player.AddItem(item, rate))
                {
                    client.DisplaySubtitle($"Vous avez creusé ~r~ {rate} {item.name}", 5000);
                    client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                }
                else

                    client.DisplayHelp("Plus de place dans votre inventaire!");

                if (!WorkingPlayers.TryRemove(client.Id, out IPlayer voided))
                    Alt.Server.LogError("Can't remove player " + client.GetPlayerHandler().PID + " WTF (Cuivre.cs)");
            });

        }


        public override void StartProcessing(IPlayer client)
        { 
            try
            {
                PlayerHandler player = client.GetPlayerHandler();
                Item item = Inventory.Inventory.ItemByID(ItemIDProcess);
                Tool _item = (Tool)(player.OutfitInventory.HasItemEquip(ItemID.Soufflet)?.Item);
                if (player.BagInventory.CountItem(ItemID.Sable) < Process_QuantityNeeded * _item.MiningRate && player.PocketInventory.CountItem(ItemID.Sable) < Process_QuantityNeeded * _item.MiningRate)
                {
                    client.DisplayHelp("Vous n'avez rien à souffler");
                    return;
                }



                DoubleProcessPoints.ForEach((p) =>
                {
                    if (p.Position.DistanceTo2D(client.Position.ConvertToVector3()) < 2)
                        client.TaskStartScenarioAtPosition("WORLD_HUMAN_WELDING", p.Position, p.Heading, Process_Time, false, false);
                });

                if (!WorkingPlayers.TryAdd(client.Id, client))
                    Alt.Server.LogError("Error to add player in working players");

                Utils.Utils.Delay((int)(Process_Time / _item.Speed), () =>
                {

                    if (!client.Exists)
                        return;
                    if (player.DeleteAllItem(ItemIDBrute, Process_QuantityNeeded * _item.MiningRate))
                    {
                        client.Freeze(false);
                        client.DisplaySubtitle($"Vous avez soufflé ~r~ {_item.MiningRate} {item.name}", 5000);
                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                        player.AddItem(item, _item.MiningRate);
                        client.StopAnimation();
                        player.UpdateFull();
                    }
                    else
                        Alt.Server.LogError("Sable.cs | Error when trying to remove items from Processing |");


                    if (!WorkingPlayers.TryRemove(client.Id, out IPlayer voided))
                        Alt.Server.LogError("Can't remove player " + client.GetPlayerHandler().PID + " WTF (Sable.cs)");
                });
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Sable Farm | StartProcessing | " + ex.Data);
            }

        }
        public override void StartDoubleProcessing(IPlayer client)
        {
            try
            {
                PlayerHandler player = client.GetPlayerHandler();
                Item item = Inventory.Inventory.ItemByID(ItemID.BouteilleTraite);
                if (player.BagInventory.CountItem(ItemID.Bouteille) < 1 && player.PocketInventory.CountItem(ItemID.Bouteille) < 1)
                {
                    client.DisplayHelp("Vous n'avez rien à stériliser");
                    return;
                }

                client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)49);
                WorkingPlayers.TryAdd(client.Id, client);
                Utils.Utils.Delay((int)(DoubleProcess_Time), () =>
                {
                    if (!client.Exists)
                        return;
                    if (player.DeleteAllItem(ItemID.Bouteille, 1))
                    {
                        client.DisplaySubtitle($"Vous avez stérilisé ~r~ 1 {item.name}", 5000);
                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                        player.AddItem(item, 1);
                        client.StopAnimation();
                        player.UpdateFull();


                        if (!WorkingPlayers.TryRemove(client.Id, out IPlayer voided))
                            Alt.Server.LogError("Can't remove player " + client.GetPlayerHandler().PID + " WTF (Sable.cs)");
                    }
                    else
                    {
                        Alt.Server.LogError("Eror on double process at Sable ");
                    }
                });
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Sable Farm | StartDoubleProcess | " + ex.Data);
            }

        }

    }
}
