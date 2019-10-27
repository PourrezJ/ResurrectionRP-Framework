using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

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
        #region Fields
        private Farm Farm;

        public Vector3 Position;
        public float Heading;
        public Item ReturnedItem;
        public float DoubleLuck = 0.0f; // Un randomint sera fait pour savoir si on veut 2 minerais
        public IColshape Colshape;
        public List<Item> ToolNeeded;
        public InteractionPointTypes Type;
        public string InteractionName;
        #endregion

        #region Constructors
        public InteractionPoint(Farm farm, Vector3 position, float heading, List<Item> items, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            DoubleLuck = doubleLuck;
            ToolNeeded = items;
            Type = interactionPoint;
            InteractionName = interactionName;
            Farm = farm;
            Heading = heading;
            Init();
        }

        public InteractionPoint(Farm farm, Vector3 position, float heading, Item item, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            DoubleLuck = doubleLuck;
            ToolNeeded = new List<Item>();
            ToolNeeded.Add(item);
            Type = interactionPoint;
            InteractionName = interactionName;
            Farm = farm;
            Heading = heading;
            Init();
        }
        #endregion

        #region Init
        private void Init()
        {
            Colshape = ColshapeManager.CreateCylinderColshape(Position - new Vector3(0, 0, 1), 2, 3);
            Colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;
            Colshape.OnPlayerInteractInColshape += Colshape_OnPlayerInteractInColshape;

            Marker.CreateMarker(MarkerType.MarkerTypeHorizontalBars, Position + new Vector3(0, 0, 1), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(128, 255, 69, 0));
        }
        #endregion

        #region Event handlers
        private void Colshape_OnPlayerInteractInColshape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                client.DisplayHelp("Vous ne pouvez être ici avec un véhicule!", 5000);
                return;
            }

            if (Farm.FarmTimers.ContainsKey(client) || (Farm).WorkingPlayers.ContainsKey(client.Id) || Farm.DoubleProcessTimers.ContainsKey(client))
                return;

            try
            {
                foreach (Item _item in ToolNeeded)
                {
                    PlayerHandler _client = client.GetPlayerHandler();
                    Inventory.Inventory inventory = _client.HasItemInAnyInventory(_item.id);
                    ItemStack item = _client.OutfitInventory.HasItemEquip(_item.id);

                    if (item != null)
                    {
                        if (item.Item.type == "tool" && (item.Item as Tool).Health <= 0)
                        {
                            _client.OutfitInventory.Delete(item, 1);
                            client.DisplayHelp("Votre outil s'est cassé, vous êtes bon pour en racheter un !", 10000);
                            return;
                        }

                        switch (Type)
                        {
                            case InteractionPointTypes.Farm:
                                Farm?.StartFarming(client);
                                return;
                            case InteractionPointTypes.DoubleProcess:
                                Farm?.StartDoubleProcessing(client);
                                return;
                            case InteractionPointTypes.Process:
                                Farm?.StartProcessing(client);
                                return;
                            case InteractionPointTypes.Sell:
                                Farm?.StartSelling(client);
                                return;
                            default:
                                Alt.Server.LogError("Problem, an unknwn Interactin type in InteractionPoints! ");
                                return;
                        }
                    }

                    if (inventory != null && item == null)
                        client.DisplayHelp("Vous devez équiper votre outil pour commencer!", 5000);
                    else if (item == null && ToolNeeded.IndexOf(_item) == ToolNeeded.Count - 1)
                    {
                        client.DisplayHelp($"Vous devez avoir un(e) {ToolNeeded[0].name} pour {InteractionName} !", 10000);
                        return;
                    }

                }
            }
            catch (System.Exception ex)
            {
                Alt.Server.LogError("InteractionPoint interact Colshape: " + ex.Data);
            }
        }

        private void Colshape_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                client.DisplayHelp("Vous ne pouvez faire être ici avec un véhicule!", 5000);
                return;
            }

            try
            {
                foreach(Item item in ToolNeeded) 
                {
                    PlayerHandler ph = client.GetPlayerHandler();
                    Inventory.Inventory inventory = ph.HasItemInAnyInventory(item.id);
                    ItemStack itemStack = ph.OutfitInventory.HasItemEquip(item.id);

                    if(itemStack != null)
                    {
                        if (itemStack.Item.type == "tool"  && (itemStack.Item as Tool).Health <= 0)
                        {
                            ph.OutfitInventory.Delete(itemStack, 1);
                            client.DisplayHelp("Votre outil s'est cassé, vous êtes bon pour en racheter un !", 10000);
                            return;
                        }

                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à " + InteractionName, 5000);
                        return;
                    }


                    if (inventory != null && itemStack == null)
                        client.DisplayHelp("Vous devez équiper votre outil pour commencer!", 5000);
                    else if (itemStack == null && ToolNeeded.IndexOf(item) == ToolNeeded.Count - 1)
                    {
                        client.DisplayHelp($"Vous devez avoir un(e) {ToolNeeded[0].name} pour {InteractionName} !", 10000);
                        return;
                    }
                };

            }
            catch (Exception ex)
            {
                Alt.Server.LogError("InteractionPoint enter colshape: " + ex.Data);
            }
        }
        #endregion
    }
}
