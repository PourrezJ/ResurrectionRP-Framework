using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Wine : Farm
    {
        public Wine()
        {
/*            Harvest_Name = "Vignoble";
            Process_Name = "Traitement du raisin";
            Selling_Name = "Revendeur de jus de raisin";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(-1802.502f, 2167.386f, 113.5449f);
            Harvest_Position.Add(new Vector3(-1802.502f, 2167.386f, 113.5449f));

            Process_PosRot = new Location(new Vector3(1545.689f, 2175.608f, 78.80177f), new Vector3(0, 0, 92.64625f));
            Selling_PosRot = new Location(new Vector3(-1553.488f, 1374.383f, 126.7967f), new Vector3(0, 0, 44.8825f));

            Harvest_Range = 50f;

            Process_PedHash = AltV.Net.Enums.PedModel.Dockwork01SMY;
            Selling_PedHash = AltV.Net.Enums.PedModel.Genstreet01AMO;


*/

            NewFarm = true;
            Harvest_Name = "Vignoble";
            Process_Name = "Traitement du raisin";
            Selling_Name = "Revendeur de jus de raisin";

            Harvest_BlipSprite = 85;

            Harvest_Time = 5000;
            Harvest_BlipPosition = new Vector3(-1808.093f, 2152.776f, 117.499f);
            Harvest_Position.Add(new Vector3(-1799.283f, 2157.733f, 117.127f));
            Harvest_Position.Add(new Vector3(-1800.554f,2146.997f, 120.006f));
            Harvest_Position.Add(new Vector3(-1814.016f, 2140.724f, 121.707f));
            Harvest_Position.Add(new Vector3(-1818.233f, 2143.723f, 119.93f));
            Harvest_Position.Add(new Vector3(-1830.712f, 2137.913f, 121.932f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, new List<Item> { Inventory.Inventory.ItemByID(ItemID.Filet), Inventory.Inventory.ItemByID(ItemID.Seau) }, InteractionPointTypes.Farm, "récolter", "amb@world_human_gardener_plant@female@base", "base_female", noNeedInSlot: true))
            );
            Harvest_Range = 100f;



            new List<Vector3>
            {
                new Vector3(1983.066f, 5172.491f, 47.639f),
                new Vector3(1980.639f, 5170.047f, 47.639f),
            }.ForEach((position) =>
                ProcessPoints.Add(new InteractionPoint(this, position, 90, InteractionPointTypes.Process, "préparer le jus"))
            );

            new List<Vector3>
            {
                new Vector3(1962.992f, 5176.313f, 47.912f),
                new Vector3(1962.409f, 5181.496f, 47.959f),
            }.ForEach((position) =>
                DoubleProcessPoints.Add(new InteractionPoint(this, position, 90, InteractionPointTypes.DoubleProcess, "faire du jus"))
            );

            ConcurrentDictionary<double, Item> eligiblelist = new ConcurrentDictionary<double, Item>();
            eligiblelist.TryAdd(75, Inventory.Inventory.ItemByID(ItemID.GrapeJuice));

            SellingPoints.Add(new InteractionPoint(this, new Vector3(-2185.827f, 4264.839f, 48.951f), -11.52f, AltV.Net.Enums.PedModel.Cntrybar01SMM, eligiblelist, InteractionPointTypes.Sell, "vendre"));


            BlipColor = (BlipColor)15;

            Process_Blip = BlipsManager.CreateBlip(Process_Name, new Vector3(1977.365f, 5171.179f, 47.639f), (byte)BlipColor, 499);
            Selling_Blip = BlipsManager.CreateBlip(Selling_Name, new Vector3(-2185.827f, 4264.839f, 48.951f), (byte)BlipColor, 500);
            Process_QuantityNeeded = 2;
            Process_Time = 5000;


            ItemIDBrute = ItemID.Raisin;
            ItemIDProcess = ItemID.GrapeJuice;
            ItemPrice = 75;
        }

        public override void StartFarmingNew(IPlayer client, Item sentItem = null, string anim_dict = null, string anim_anim = null, string scenario = null)
        {
            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();


            if (player == null || player.IsOnProgress)
                return;

            Item endItem = Inventory.Inventory.ItemByID(ItemIDBrute);
            int rate = player.HasItemID(ItemID.Seau) ? 5 : 1;


            if (player.InventoryIsFull(endItem.weight))
            {
                client.DisplayHelp("Votre inventaire est déjà plein.", 10000);
                return;
            }

            client.DisplayHelp("Vous commencez à récolter...", Harvest_Time);

            player.IsOnProgress = true;

            if (anim_anim != "" & anim_dict != "")
                client.PlayAnimation(anim_dict, anim_anim, 8, -1, Harvest_Time, (Utils.Enums.AnimationFlags)1);

            Utils.Utils.Delay((int)Harvest_Time, () =>
            {

                if (!client.Exists)
                    return;

                if (player.AddItem(endItem, rate))
                {
                    client.DisplaySubtitle($"Vous avez récolté ~r~ {rate} {endItem.name}", 5000);
                    client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                }
                else

                    client.DisplayHelp("Plus de place dans votre inventaire!");
                player.IsOnProgress = false;

            });
        }

        public override void StartProcessing(IPlayer client)
        {

            PlayerHandler player = client.GetPlayerHandler();
            if (player.IsOnProgress)
                return;

            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"{client.GetPlayerHandler().PID} is starting to create jus de raisin");

            if (player.CountItem(ItemID.Raisin) <= 2)
            {
                client.DisplayHelp("Vous n'avez pas assez de raisin");
                return;
            }

            Item item = Inventory.Inventory.ItemByID(ItemID.RaisinLiquide);
            client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
            player.IsOnProgress = true;

            Utils.Utils.Delay((int)(Process_Time), () =>
            {
                if (!client.Exists)
                    return;

                if (player.DeleteAllItem(ItemID.Raisin, 2))
                {
                    client.DisplaySubtitle($"Vous avez créé ~r~ 1 {item.name}", 5000);
                    client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~  pour recommencer", 5000);
                    player.AddItem(item, 1);
                    client.StopAnimation();
                    player.UpdateFull();


                    player.IsOnProgress = false;

                }
                else
                    Alt.Server.LogError("Error on deletng Raisin Wine ");
            });
        }
        public override void StartDoubleProcessing(IPlayer client)
        {

            PlayerHandler player = client.GetPlayerHandler();
            if (player.IsOnProgress)
                return;

            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"{client.GetPlayerHandler().PID} is starting to finish jus de raisin");

            if (player.CountItem(ItemID.RaisinLiquide) <= 0)
            {
                client.DisplayHelp("Vous n'avez pas assez de raisin liquide");
                return;
            }

            Item item = Inventory.Inventory.ItemByID(ItemID.GrapeJuice);
            client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
            player.IsOnProgress = true;

            Utils.Utils.Delay((int)(Process_Time), () =>
            {
                if (!client.Exists)
                    return;

                if (player.DeleteAllItem(ItemID.RaisinLiquide, 1))
                {
                    client.DisplaySubtitle($"Vous avez créé ~r~ 1 {item.name}", 5000);
                    client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~  pour recommencer", 5000);
                    player.AddItem(item, 1);
                    client.StopAnimation();
                    player.UpdateFull();


                    player.IsOnProgress = false;

                }
                else
                    Alt.Server.LogError("Error on deletng Raisin Wine ");
            });
        }
    }
}