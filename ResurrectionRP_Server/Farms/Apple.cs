using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    class Apple : Farm
    {
        public Apple()
        {

/*
            Harvest_BlipPosition = new Vector3(349.9534f, 6517.268f, 28.60002f);
            Harvest_Range = 3;
            Harvest_Position.Add(new Vector3(377.5282f, 6506.09f, 27.9903f));
            Harvest_Position.Add(new Vector3(370.7609f, 6505.783f, 28.38556f));
            Harvest_Position.Add(new Vector3(363.1317f, 6506.318f, 28.53325f));
            Harvest_Position.Add(new Vector3(355.7483f, 6505.399f, 28.47935f));
            Harvest_Position.Add(new Vector3(347.4283f, 6505.96f, 28.83254f));
            Harvest_Position.Add(new Vector3(340.8368f, 6505.983f, 28.73354f));
            Harvest_Position.Add(new Vector3(331.1703f, 6506.094f, 28.53302f));
            Harvest_Position.Add(new Vector3(321.9208f, 6505.947f, 29.20082f));
            Harvest_Position.Add(new Vector3(322.0467f, 6516.95f, 29.1247f));
            Harvest_Position.Add(new Vector3(329.7644f, 6517.089f, 28.98491f));
            Harvest_Position.Add(new Vector3(338.9037f, 6516.58f, 28.94523f));
            Harvest_Position.Add(new Vector3(346.8972f, 6517.256f, 28.83298f));
            Harvest_Position.Add(new Vector3(355.3142f, 6516.765f, 28.19206f));
            Harvest_Position.Add(new Vector3(362.1714f, 6517.205f, 28.25648f));
            Harvest_Position.Add(new Vector3(370.5898f, 6517.313f, 28.37161f));
            Harvest_Position.Add(new Vector3(377.677f, 6517.756f, 28.3777f));
            Harvest_Position.Add(new Vector3(369.7759f, 6531.406f, 28.38906f));
            Harvest_Position.Add(new Vector3(362.0231f, 6531.524f, 28.33606f));
            Harvest_Position.Add(new Vector3(354.1955f, 6531.298f, 28.32363f));
            Harvest_Position.Add(new Vector3(346.2615f, 6530.881f, 28.70708f));
            Harvest_Position.Add(new Vector3(346.2615f, 6530.881f, 28.70708f));
            Harvest_Position.Add(new Vector3(338.6309f, 6531.786f, 28.55268f));
            Harvest_Position.Add(new Vector3(329.9322f, 6530.722f, 28.58525f));
            Harvest_Position.Add(new Vector3(323.5154f, 6531.181f, 29.02637f));

            Process_PedHash = PedModel.Hillbilly01AMM;

            Selling_PosRot = new Location(new Vector3(-1043.259f, 5326.641f, 44.56397f), new Vector3(0, 0, 27.56091f));
            Selling_PedHash = PedModel.ShopKeep01;
            BlipColor = Entities.Blips.BlipColor.WineRed;

            ItemIDProcess = ItemID.Apple;
            ItemIDBrute = ItemID.Apple;
            ItemPrice = 18;

            Debug = true;*/


            NewFarm = true;
            Harvest_Name = "Champ de pommes";
            Selling_Name = "Etalage maraîcher";

            Harvest_BlipSprite = 85;

            Harvest_Time = 5000;
            Harvest_BlipPosition = new Vector3(343.902f, 6512.63f, 28.961f);
            Harvest_Position.Add(new Vector3(341.159f, 6506.343f, 28.752f));
            Harvest_Position.Add(new Vector3(331.773f, 6506.49f, 28.49f));
            Harvest_Position.Add(new Vector3(330.556f, 6517.06f, 28.972f));
            Harvest_Position.Add(new Vector3(337.988f, 6517, 28.941f));
            Harvest_Position.Add(new Vector3(346.106f, 6516.956f, 28.893f));
            Harvest_Position.Add(new Vector3(354.81f, 6516.291f, 28.249f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, InteractionPointTypes.Farm, "ramasser des pommes"))
            );
            Harvest_Range = 100f;


            ConcurrentDictionary<double, Item> eligiblelist = new ConcurrentDictionary<double, Item>();
            eligiblelist.TryAdd(18, Inventory.Inventory.ItemByID(ItemID.Apple));

            SellingPoints.Add(new InteractionPoint(this, new Vector3(-1043.343f, 5327.187f, 44.553f), -11.52f, AltV.Net.Enums.PedModel.ShopKeep01, eligiblelist, InteractionPointTypes.Sell, "vendre"));



            /*            Selling_PosRot = new Location(new Vector3(605.719f, -3073.165f, 6.069f), new Vector3(0, 0, -11.52882f));
                        Selling_PedHash = AltV.Net.Enums.PedModel.Cntrybar01SMM;*/

            BlipColor = Entities.Blips.BlipColor.LightGreen;

            Selling_Blip = BlipsManager.CreateBlip(Selling_Name, new Vector3(-1043.343f, 5327.187f, 44.553f), (byte)BlipColor, 500);

            ItemIDProcess = ItemID.Apple;
            ItemIDBrute = ItemID.Apple;
            ItemPrice = 18;
        }

        public override void StartFarming(IPlayer client)
        {

            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();


            if (player == null || player.IsOnProgress)
                return;

            Item endItem = Inventory.Inventory.ItemByID(ItemIDBrute);
            Item tool = player.HasItemID(ItemID.Filet) ? Inventory.Inventory.ItemByID(ItemID.Filet) : null ;


            if (player.InventoryIsFull(endItem.weight))
            {
                client.DisplayHelp("Votre inventaire est déjà plein.", 10000);
                return;
            }

            client.DisplayHelp("Vous commencez à récolter...", Harvest_Time);

            player.IsOnProgress = true;

            client.PlayAnimation("amb@world_human_gardener_plant@female@base", "base_female", 8, -1, Harvest_Time, (Utils.Enums.AnimationFlags)1);

            Utils.Utils.Delay( (int)Harvest_Time, () =>
            {

                if (!client.Exists)
                    return;

                if (player.AddItem(endItem, tool != null ? 3 : 1))
                {
                    client.DisplaySubtitle($"Vous avez récolté ~r~ {(tool != null ? 3 : 1)} {endItem.name}", 5000);
                    client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                }
                else

                    client.DisplayHelp("Plus de place dans votre inventaire!");
                player.IsOnProgress = false;

            });
        }
    }
}
