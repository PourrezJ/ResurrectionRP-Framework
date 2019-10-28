using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Entities.Blips;
using AltV.Net.Elements.Entities;
using AltV.Net;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;

namespace ResurrectionRP_Server.Farms
{
    public class Gold  : Farm
    {

        private static int UsureOutil = 1;

        public Gold()
        {
            NewFarm = true;
            Harvest_Name = null;
            Process_Name = null;
            Selling_Name = null;

            Harvest_BlipSprite = 0;
            Process_BlipSprite = 0;
            Selling_BlipSprite = 0;

            Harvest_Time = 5000;
            Harvest_Position.Add(new Vector3(-545.525f, 1982.322f, 127.052f));
            Harvest_Position.Add(new Vector3(-533.938f, 1903.403f, 123.091f));
            Harvest_Position.Add(new Vector3(-564.635f, 1886.305f, 123.035f));
            Harvest_Position.Add(new Vector3(-481.573f, 1895.112f, 119.686f));
            Harvest_Position.Add(new Vector3(-445.687f, 2012.592f, 123.575f));
            Harvest_Position.Add(new Vector3(-453.926f, 2054.839f, 122.183f));
            Harvest_Position.Add(new Vector3(-472.789f, 2089.373f, 120.067f));
            Harvest_Position.Add(new Vector3(-424.038f, 2064.779f, 120.047f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, Inventory.Inventory.ItemByID(ItemID.Pioche), InteractionPointTypes.Farm, "miner"))
            );
            Harvest_Range = 100f;
            
        }


        public override void StartFarming(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            if (GameMode.IsDebug)
                Alt.Server.LogInfo($"Player {client.GetPlayerHandler()?.PID} is now farming at Gold");

            PlayerHandler player = client.GetPlayerHandler();
            Tool _item = (Tool)(player.OutfitInventory.HasItemEquip(ItemID.Pioche)?.Item);



            if (player == null || player.IsOnProgress)
                return;
            if (player.InventoryIsFull(2))
            {
                client.DisplayHelp("Votre inventaire est déjà plein.", 10000);
                return;
            }
            if (_item == null)
                return;

            _item.SetPickaxeAnimation(client, (int)(Harvest_Time / _item.Speed));
            client.DisplayHelp($"Durabilité: {_item.Health - UsureOutil}\nMinerai récolté: {1}\nVitesse: {_item.Speed}", 5000);
            _item.Health -= UsureOutil;

            WorkingPlayers[client.Id] = client;
            Utils.Utils.Delay((int)(Harvest_Time / _item.Speed), () => {
                Item item = (client.GetPlayerHandler().HasItemID(ItemID.DetecteurMetaux)) ? this.RadomizedItem(5, 53) : this.RadomizedItem(1, 49);
                if (!client.Exists)
                    return;

                if (player.AddItem(item, 1))
                {
                    client.DisplaySubtitle($"Vous avez miné ~r~ {1} {item.name}", 5000);
                    WorkingPlayers.TryRemove(client.Id, out IPlayer voided);
                }
                else
                    client.DisplayHelp("Plus de place dans votre inventaire!");

            });
        }

        public Item RadomizedItem(int goldNugget, int sand)
        {
            int random = new Random().Next(1, 100);
            if (random == goldNugget)
                return Inventory.Inventory.ItemByID(ItemID.PepiteOr);
            else if (random > goldNugget && random <= sand)
                return Inventory.Inventory.ItemByID(ItemID.Sable);
            else 
                return Inventory.Inventory.ItemByID(ItemID.MineraiCuivre);
        }

    }
}
