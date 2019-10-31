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
using ResurrectionRP_Server.Colshape;

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
            Harvest_Position.Add(new Vector3(-545.525f, 1982.322f, 127.052f));s'effonde'
            Harvest_Position.Add(new Vector3(-533.938f, 1903.403f, 123.091f));
            Harvest_Position.Add(new Vector3(-564.635f, 1886.305f, 123.035f));
            Harvest_Position.Add(new Vector3(-481.573f, 1895.112f, 119.686f));
            Harvest_Position.Add(new Vector3(-445.687f, 2012.592f, 123.575f));
            Harvest_Position.Add(new Vector3(-453.926f, 2054.839f, 122.183f));
            Harvest_Position.Add(new Vector3(-472.789f, 2089.373f, 120.067f));
            Harvest_Position.Add(new Vector3(-424.038f, 2064.779f, 120.047f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, Inventory.Inventory.ItemByID(ItemID.Pioche), InteractionPointTypes.Farm, "miner", "melee@large_wpn@streamed_core", "ground_attack_on_spot"))
            );
            Harvest_Range = 100f;

            // ANTI NOOB: Les joueurs ne peuvent plus entrer dans la mine d'or avec un véhicule.
            var antinoob = ColshapeManager.CreateCylinderColshape(new Vector3(-595.2263f, 2085.837f, 130.1125f), 10, 5 );
            antinoob.OnPlayerEnterColshape += Antinoob_OnPlayerEnterColshape;
            antinoob.OnPlayerLeaveColshape += Antinoob_OnPlayerEnterColshape;        }

        private void Antinoob_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                client.Emit("SetPlayerOutOfVehicle", false);
                client.DisplayHelp("Soyez pas fou, la mine s'effondrerait si vous rentriez en véhicule.");
                Alt.Server.LogInfo("[Gold.Antinoob_OnPlayerEnterColshape()] Le joueur " + client.GetPlayerHandler().PID + " a tenté de rentrer avec un véhicule dans la mine d'or");
            }
        }

        public override void StartFarmingNew(IPlayer client, Item sentItem, string anim_dict = null, string anim_anim = null, string scenario = null)
        {
            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();
            Tool tool = sentItem as Tool;
            if (player == null || player.IsOnProgress)
                return;
            if (tool == null)
                return;

            client.DisplayHelp($"Durabilité: {tool.Health - UsureOutil}\nMinerais récoltées: {tool.MiningRate}\nVitesse: {tool.Speed}", 5000);
            tool.Health -= UsureOutil;
            player.IsOnProgress = true;
            if (anim_anim != "" & anim_dict != "")
                client.PlayAnimation(anim_dict, anim_anim, 8, -1, Harvest_Time, (Utils.Enums.AnimationFlags)1);

            if (scenario != "")

                FarmPoints.ForEach((p) =>
                {
                    if (p.Position.DistanceTo2D(client.Position.ConvertToVector3()) < 2)
                        client.TaskStartScenarioAtPosition(scenario, p.Position, p.Heading, Process_Time, false, false);
                });

            Utils.Utils.Delay((int)(Harvest_Time / tool.Speed), () =>
            {

                Item endItem = (client.GetPlayerHandler().HasItemID(ItemID.DetecteurMetaux)) ? this.RadomizedItem(5, 53) : this.RadomizedItem(1, 49);
                if (!client.Exists)
                    return;

                if (player.AddItem(endItem, tool.MiningRate))
                {
                    client.DisplaySubtitle($"Vous avez récolté ~r~ {tool.MiningRate} {endItem.name}", 5000);
                    client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour recommencer", 5000);
                }
                else

                    client.DisplayHelp("Plus de place dans votre inventaire!");
                player.IsOnProgress = false;

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
