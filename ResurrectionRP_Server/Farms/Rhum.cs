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
    public class Rhum : Farm
    {
        private static int UsureOutil = 1;
        public Rhum()
        {


            /*            Harvest_Name = "Champ de canne a sucre";
                        Process_Name = "Distillerie";
                        //Selling_Name = "Revendeur de rhum";

                        Harvest_BlipSprite = 85;
                        Process_BlipSprite = 499;

                        Harvest_BlipPosition = new Vector3(2262.616f, 4770.627f, 39.27166f);
                        Harvest_Position.Add(new Vector3(2262.616f, 4770.627f, 39.27166f));

                        Process_PosRot = new Location(new Vector3(1255.333f, -2682.066f, 2.072282f), new Vector3(0, 0, 285.0722f));

                        Harvest_Range = 10f;

                        Process_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;

                        BlipColor = (BlipColor)51;

                        ItemIDBrute = ItemID.Canneasurcre;
                        ItemIDProcess = ItemID.RhumLiquide;
                    */

            NewFarm = true;
            Harvest_Name = "Champ de canne à sucre";
            Process_Name = "Distillerie";
            DoubleProcess_Name = "Mise en bouteille (Rhum)";
            Selling_Name = "Revendeur de Rhum";

            Harvest_BlipSprite = 85;

            Harvest_BlipPosition = new Vector3(2269.544f,4771f, 39);
            Harvest_Position.Add(new Vector3(2273.261f, 4780.318f, 39.51f));
            Harvest_Position.Add(new Vector3(2271.487f, 4773.486f, 39.467f));
            Harvest_Position.Add(new Vector3(2268.82f, 4763.432f, 39.465f));
            Harvest_Position.Add(new Vector3(2264.374f, 4764.073f, 39.465f));
            Harvest_Position.Add(new Vector3(2266.599f, 4770.905f, 39.521f));
            Harvest_Position.Add(new Vector3(2268.723f, 4778.318f, 39.538f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, new List<Item>{ Inventory.Inventory.ItemByID(ItemID.OutilMachette), Inventory.Inventory.ItemByID(ItemID.OutilCouteau) }, InteractionPointTypes.Farm, "récolter", "amb@world_human_gardener_plant@female@base", "base_female"))
            );


            new List<Vector3>
            {
                new Vector3(1254.998f, -2684.194f, 2.12f),
                new Vector3(1253.953f, -2678.991f, 2.052f),
                new Vector3(1250.083f, -2678.791f, 2.138f)
            }.ForEach((position) =>
                ProcessPoints.Add(new InteractionPoint(this, position, -2.3f,  InteractionPointTypes.Process, "travailler le rhum", "amb@prop_human_parking_meter@male@idle_a","idle_a"))
            );

            new List<Vector3>
            {
                new Vector3(-617.969f, -1628.808f, 32.5f),
                new Vector3(-610.651f, -1629.208f, 32.5f),
                new Vector3(-614.732f, -1632.952f, 32.5f)
            }.ForEach((position) =>
                DoubleProcessPoints.Add(new InteractionPoint(this, position, 90, InteractionPointTypes.DoubleProcess, "mettre en bouteille"))
            );

            ConcurrentDictionary<double, Item> eligiblelist = new ConcurrentDictionary<double, Item>();
            eligiblelist.TryAdd(526, Inventory.Inventory.ItemByID(ItemID.Rhum));
            eligiblelist.TryAdd(546, Inventory.Inventory.ItemByID(ItemID.RhumRaisin));
            eligiblelist.TryAdd(546, Inventory.Inventory.ItemByID(ItemID.RhumApple));

            new List<Vector3>
            {
                new Vector3(-1137.303f, -1251.585f, 7.084f),
            }.ForEach((position) =>

               SellingPoints.Add(new InteractionPoint(this, position, 285, AltV.Net.Enums.PedModel.Beach03AMY, eligiblelist, InteractionPointTypes.Sell, "vendre"))
           );


            Harvest_Range = 50f;

            BlipColor = (BlipColor)51;
            Process_Blip = BlipsManager.CreateBlip(Process_Name, new Vector3(1253.476f, -2681.868f, 2.11f), BlipColor, 499);
            DoubleProcess_Blip = BlipsManager.CreateBlip(DoubleProcess_Name, new Vector3(-617.586f, -1631.681f, 33.2f), BlipColor, 499);
            Selling_Blip = BlipsManager.CreateBlip(Selling_Name, new Vector3(-1137.303f, -1251.585f, 7.084f), BlipColor, 500);

            ItemIDBrute = ItemID.Canneasurcre;
            ItemIDProcess = ItemID.RhumLiquide;


            Harvest_Time = 5000;
            Process_Time = 5000;
            DoubleProcess_Time = 5000;

        }


        public override void StartProcessing(IPlayer client)
        {
            try
            {
                PlayerHandler player = client.GetPlayerHandler();
                if (player.CountItem(ItemID.Canneasurcre) <= 0 && player.CountItem(ItemID.Melasse) <= 0)
                    return;

                Menu menu = new Menu("ID_ProcessRhum", "Rhum", "Créez votre rhum", backCloseMenu: true);

                MenuItem menuItem = new MenuItem("Faire de la mélasse", "Consommer une canne à sucre pour de la mélasse", "ID_Melasse", true);
                menuItem.OnMenuItemCallback = Melasser;
                menu.Add(menuItem);

                menuItem = new MenuItem("Faire mélasse arrangé pomme", "Prend 1 pomme & 1 mélasse à faire", "ID_RhumPomme", true);
                menuItem.OnMenuItemCallback = MelassePomme;
                menu.Add(menuItem);

                menuItem = new MenuItem("Faire mélasse arrangé raisin", "Prend 1 raisin & 1 mélasse à faire", "ID_RhumRaisin", true);
                menuItem.OnMenuItemCallback = MelasseRaisin;
                menu.Add(menuItem);
                


                menu.OpenMenu(client);
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Rhum Farm | StartProcessing | " + ex.Data);
            }

        }

        private void Melasser(IPlayer client, Menu menu = null, IMenuItem menuItem = null, int itemIndex = 0)
        {
            PlayerHandler player = client.GetPlayerHandler();
            if (player.IsOnProgress)
                return;
            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"{client.GetPlayerHandler().PID} is starting melasser at Rhum");
            if (player.CountItem(ItemID.Canneasurcre) <= 0)
            {
                client.DisplayHelp("Vous n'avez pas assez de mélasse");
                return;
            }

            Item item = Inventory.Inventory.ItemByID(ItemID.Melasse);
            client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
            player.IsOnProgress = true;

            Utils.Utils.Delay((int)(Process_Time), () =>
            {
                if (!client.Exists)
                    return;
                if (player.DeleteAllItem(ItemID.Canneasurcre, 1))
                {
                    client.DisplaySubtitle($"Vous avez créé ~r~ 1 {item.name}", 5000);
                    client.DisplayHelp("Appuyez sur Entrer  pour recommencer", 5000);
                    player.AddItem(item, 1);
                    client.StopAnimation();
                    player.UpdateFull();


                    player.IsOnProgress = false;

                }
                else
                    Alt.Server.LogError("Eror on mélasser process at Rhum ");
            });

        }
        private void MelassePomme(IPlayer client, Menu menu = null, IMenuItem menuItem = null, int itemIndex = 0)
        {
            PlayerHandler player = client.GetPlayerHandler();
            if (player.IsOnProgress)
                return;
            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"{client.GetPlayerHandler().PID} is starting melasser pomme at Rhum");
            if (player.CountItem(ItemID.Melasse) <= 0 || player.CountItem(ItemID.Apple) <= 0)
            {
                client.DisplayHelp("Vous n'avez pas de quoi faire ça !");
                return;
            }

            Item item = Inventory.Inventory.ItemByID(ItemID.MelasseApple);
            client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
            player.IsOnProgress = true;

            Utils.Utils.Delay((int)(Process_Time), () =>
            {
                if (!client.Exists)
                    return;
                if (player.DeleteAllItem(ItemID.Melasse, 1) && player.DeleteAllItem(ItemID.Apple, 1))
                {
                    client.DisplaySubtitle($"Vous avez créé ~r~ 1 {item.name}", 5000);
                    client.DisplayHelp("Appuyez sur Entrer  pour recommencer", 5000);
                    player.AddItem(item, 1);
                    client.StopAnimation();
                    player.UpdateFull();

                    player.IsOnProgress = false;
                }
                else
                    Alt.Server.LogError("Eror on mélasser process at Rhum ");
            });
        }
        private void MelasseRaisin(IPlayer client, Menu menu = null, IMenuItem menuItem = null, int itemIndex = 0)
        {
            PlayerHandler player = client.GetPlayerHandler();
            if (player.IsOnProgress)
                return;
            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"{client.GetPlayerHandler().PID} is starting melasser raisin at Rhum");
            if (player.CountItem(ItemID.Melasse) <= 0 || player.CountItem(ItemID.Raisin) <= 0)
            {
                client.DisplayHelp("Vous n'avez pas de quoi faire ça !");
                return;
            }

            Item item = Inventory.Inventory.ItemByID(ItemID.MelasseRaisin);
            client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
            player.IsOnProgress = true;

            Utils.Utils.Delay((int)(Process_Time), () =>
            {
                if (!client.Exists)
                    return;
                if (player.DeleteAllItem(ItemID.Melasse, 1) && player.DeleteAllItem(ItemID.Raisin, 1))
                {
                    client.DisplaySubtitle($"Vous avez créé ~r~ 1 {item.name}", 5000);
                    client.DisplayHelp("Appuyez sur Entrer  pour recommencer", 5000);
                    player.AddItem(item, 1);
                    client.StopAnimation();
                    player.UpdateFull();

                    player.IsOnProgress = false;
                }
                else
                    Alt.Server.LogError("Eror on mélasser process at Rhum ");
            });
        }
        public override void StartDoubleProcessing(IPlayer client)
        {
            try
            {
                PlayerHandler player = client.GetPlayerHandler();
                if(player.CountItem(ItemID.BouteilleTraite) <= 0)
                {
                    client.DisplayHelp("Vous n'avez pas de bouteille stérilisée");
                    return;
                }
                if (player.CountItem(ItemID.MelasseApple) <= 0 && player.CountItem(ItemID.MelasseRaisin) <= 0 && player.CountItem(ItemID.Melasse) <= 0)
                {
                    client.DisplayHelp("Vous n'avez rien à embouteiller");
                    return;
                }
                Item item = null;
                if (player.CountItem(ItemID.MelasseApple) > 0)
                    item = Inventory.Inventory.ItemByID(ItemID.MelasseApple);
                else if (player.CountItem(ItemID.MelasseRaisin) > 0)
                    item = Inventory.Inventory.ItemByID(ItemID.MelasseRaisin);
                else if (player.CountItem(ItemID.Melasse) > 0)
                    item = Inventory.Inventory.ItemByID(ItemID.Melasse);

                client.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
                player.IsOnProgress = true;

                Utils.Utils.Delay((int)(DoubleProcess_Time), () =>
                {
                    if (!client.Exists)
                        return;
                    Item finalItem = null;
                    if (player.CountItem(ItemID.MelasseApple) > 0)
                        finalItem = Inventory.Inventory.ItemByID(ItemID.RhumApple);
                    else if (player.CountItem(ItemID.MelasseRaisin) > 0)
                        finalItem = Inventory.Inventory.ItemByID(ItemID.RhumRaisin);
                    else if (player.CountItem(ItemID.Melasse) > 0)
                        finalItem = Inventory.Inventory.ItemByID(ItemID.Rhum);

                    if (player.DeleteAllItem(ItemID.BouteilleTraite, 1) && player.DeleteAllItem(item.id, 1))
                    {
                        client.DisplaySubtitle($"Vous avez confectionné ~r~ 1 {finalItem.name}", 5000);
                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                        player.AddItem(finalItem, 1);
                        client.StopAnimation();
                        player.UpdateFull();


                        player.IsOnProgress = false;

                    }
                    else
                        Alt.Server.LogError("Eror on double process at Rhum ");
                });
            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("Rhum Farm | StartDoubleProcess | " + ex.Data);
            }

        }

    }
}

