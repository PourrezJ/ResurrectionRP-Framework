using System;
using System.Collections.Generic;
using System.Numerics;
using AltV.Net;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;

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
        public float Heading;
        public Item ReturnedItem;
        public float DoubleLuck = 0.0f; // Un randomint sera fait pour savoir si on veut 2 minerais
        public IColshape Colshape;

        public List<Item> ToolNeeded;
        public InteractionPointTypes Type;
        public string InteractionName;

        public InteractionPoint(Farm farm, Vector3 position, float heading, Item returnedItem, List<Item> items, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            ReturnedItem = returnedItem;
            DoubleLuck = doubleLuck;
            ToolNeeded = items;
            Type = interactionPoint;
            InteractionName = interactionName;
            Farm = farm;
            Heading = heading;
            Init();
        }
        public InteractionPoint(Farm farm, Vector3 position, float heading, Item returnedItem, Item item, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            ReturnedItem = returnedItem;
            DoubleLuck = doubleLuck;
            ToolNeeded = new List<Item> { item };
            Type = interactionPoint;
            InteractionName = interactionName;
            Farm = farm;
            Heading = heading;
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
            if (Farm.FarmTimers.ContainsKey(client) || (Farm as Cuivre).WorkingPlayers.ContainsKey(client.Id) || Farm.DoubleProcessTimers.ContainsKey(client))
                return;
            try
            {
                ToolNeeded.ForEach((_item) =>
                {
                    PlayerHandler _client = client.GetPlayerHandler();
                    Models.ItemStack item = _client.OutfitInventory.HasItemEquip(_item.id);

                    if (client.IsInVehicle)
                        client.DisplayHelp("Vous ne pouvez faire être ici avec un véhicule!", 5000);
                    else if (item == null && ToolNeeded.IndexOf(_item) != ToolNeeded.Count - 1)
                        client.DisplayHelp($"Vous devez avoir un(e) {_item.name} pour {InteractionName} !", 10000);
                    else if ((item != null && item.Item.type == "pickaxe" && (item.Item as Pickaxe).Health <= 0))
                    {
                        _client.OutfitInventory.Delete(item, 1);
                        client.DisplayHelp("Votre outil s'est cassé, vous êtes bon pour en racheter un !", 10000);
                    }
                    else if (item != null)
                    {

                        switch (Type)
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
                    else if (item == null && ToolNeeded.IndexOf(_item) != ToolNeeded.Count - 1)
                        client.DisplayHelp($"Vous devez avoir un(e) {_item.name} pour {InteractionName} !", 10000);
                    else if ((item != null && item.Item.type == "pickaxe" && (item.Item as Pickaxe).Health <= 0))
                    {
                        _client.OutfitInventory.Delete(item, 1);
                        client.DisplayHelp("Votre outil s'est cassé, vous êtes bon pour en racheter un !", 10000);
                    }
                    else if (item != null)
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

}
