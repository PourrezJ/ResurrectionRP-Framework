using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
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

        #region Constructor
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
        public InteractionPoint(Farm farm, Vector3 position, float heading, InteractionPointTypes interactionPoint, string interactionName, float doubleLuck)
        {
            Position = position;
            DoubleLuck = doubleLuck;
            ToolNeeded = new List<Item>();
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

        #region Event Handlers
        private void Colshape_OnPlayerInteractInColshape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                client.DisplayHelp("Vous ne pouvez faire être ici avec un véhicule!", 5000);
                return;
            }
            if (Farm.FarmTimers.ContainsKey(client) || (Farm).WorkingPlayers.ContainsKey(client.Id) || Farm.DoubleProcessTimers.ContainsKey(client))
                return;
            if (ToolNeeded.Count == 0)
                LaunchToFarm(client);
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
                        LaunchToFarm(client);
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

                Alt.Server.LogError("Interaction Point Interact Colshape: " + ex.Data);
            }
        }

        private void Colshape_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                client.DisplayHelp("Vous ne pouvez faire être ici avec un véhicule!", 5000);
                return;
            }
            if (ToolNeeded.Count == 0)
                client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à " + InteractionName, 5000);

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

                        client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à " + InteractionName, 5000);
                        return;
                    }


                    if (inventory != null && item == null)
                        client.DisplayHelp("Vous devez équiper votre outil pour commencer!", 5000);
                    else if (item == null && ToolNeeded.IndexOf(_item) == ToolNeeded.Count - 1)
                    {
                        client.DisplayHelp($"Vous devez avoir un(e) {ToolNeeded[0].name} pour {InteractionName} !", 10000);
                        return;
                    }
                };

            }
            catch (System.Exception ex)
            {

                Alt.Server.LogError("InteractionPoint  enter colshape: " + ex.Data);
            }

        }
        #endregion

        #region Methods
        private void LaunchToFarm(IPlayer client)
        {
            PlayerHandler _client = client.GetPlayerHandler();
            switch (Type)
            {
                case InteractionPointTypes.Farm:
                    Alt.Server.LogInfo("InteractionPoint | " + _client.PID + " a commence a farm " + Farm.Harvest_Name);
                    Farm?.StartFarming(client);
                    return;
                case InteractionPointTypes.DoubleProcess:
                    Alt.Server.LogInfo("InteractionPoint | " + _client.PID + " a commence a double process " + Farm.DoubleProcess_Name);
                    Farm?.StartDoubleProcessing(client);
                    return;
                case InteractionPointTypes.Process:
                    Alt.Server.LogInfo("InteractionPoint | " + _client.PID + " a commence a process " + Farm.Process_Name);
                    Farm?.StartProcessing(client);
                    return;
                case InteractionPointTypes.Sell:
                    Alt.Server.LogInfo("InteractionPoint | " + _client.PID + " a commence a vendre " + Farm.Selling_Name);
                    Farm?.StartSelling(client);
                    return;
                default:
                    Alt.Server.LogError("Problem, an unknwn Interactin type in InteractionPoints! ");
                    return;
            }
        }
        #endregion
    }

}
