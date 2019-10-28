using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Concurrent;
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
        private Farm _farm;

        public Vector3 Position;
        public float Heading;
        public IColshape Colshape;
        public List<Item> ToolNeeded;
        public InteractionPointTypes Type;
        public string InteractionName;

        public ConcurrentDictionary<double, Item> soldItems = new ConcurrentDictionary<double, Item>();
        public PedModel PedModel;
        #endregion

        #region Constructors
        public InteractionPoint(Farm farm, Vector3 position, float heading, List<Item> items, InteractionPointTypes interactionPoint, string interactionName)
        {
            Position = position;
            ToolNeeded = items;
            Type = interactionPoint;
            InteractionName = interactionName;
            _farm = farm;
            Heading = heading;
            Init();
        }

        public InteractionPoint(Farm farm, Vector3 position, float heading, Item item, InteractionPointTypes interactionPoint, string interactionName)
        {
            Position = position;
            ToolNeeded = new List<Item>();
            ToolNeeded.Add(item);
            Type = interactionPoint;
            InteractionName = interactionName;
            _farm = farm;
            Heading = heading;
            Init();
        }

        public InteractionPoint(Farm farm, Vector3 position, float heading, InteractionPointTypes interactionPoint, string interactionName)
        {
            Position = position;
            ToolNeeded = new List<Item>();
            Type = interactionPoint;
            InteractionName = interactionName;
            _farm = farm;
            Heading = heading;
            Init();
        }

        public InteractionPoint(Farm farm, Vector3 position, float heading, PedModel pedmodel,ConcurrentDictionary<double, Item> items, InteractionPointTypes interactionPoint, string interactionName)
        {
            if(interactionPoint != InteractionPointTypes.Sell)
            {
                Alt.Server.LogError("InteractionPoint | Type is not about selling point, but got its constructor ");
                return;
            }

            Position = position;
            ToolNeeded = new List<Item>();
            soldItems = items;
            Type = interactionPoint;
            InteractionName = interactionName;
            _farm = farm;
            Heading = heading;
            PedModel = pedmodel;
            Init();
        }
        #endregion

        #region Init
        private void Init()
        {
            if(Type == InteractionPointTypes.Sell )
            {
                Ped ped = Ped.CreateNPC(PedModel, Position, Heading);
                ped.NpcInteractCallBack = NpcInteractSell;
            }
            else
            {
                Colshape = ColshapeManager.CreateCylinderColshape(Position - new Vector3(0, 0, 1), 2, 3);
                Colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;
                Colshape.OnPlayerInteractInColshape += Colshape_OnPlayerInteractInColshape;

                Marker.CreateMarker(MarkerType.MarkerTypeHorizontalBars, Position + new Vector3(0, 0, 1), new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(128, 255, 69, 0));
            }
        }
        #endregion

        #region Event Handlers
        private void Colshape_OnPlayerInteractInColshape(IColshape colshape, IPlayer client)
        {
            if (client.IsInVehicle)
            {
                client.DisplayHelp("Vous ne pouvez être ici avec un véhicule!", 5000);
                return;
            }
            else if (_farm.FarmTimers.ContainsKey(client) || (_farm).WorkingPlayers.ContainsKey(client.Id) || _farm.DoubleProcessTimers.ContainsKey(client))
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

            if (ToolNeeded.Count == 0)
                client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour commencer à " + InteractionName, 5000);

            try
            {
                foreach (Item item in ToolNeeded)
                {
                    PlayerHandler ph = client.GetPlayerHandler();
                    Inventory.Inventory inventory = ph.HasItemInAnyInventory(item.id);
                    ItemStack itemStack = ph.OutfitInventory.HasItemEquip(item.id);

                    if (itemStack != null)
                    {
                        if (itemStack.Item.type == "tool" && (itemStack.Item as Tool).Health <= 0)
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
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("InteractionPoint enter colshape: " + ex.Data);
            }
        }
        #endregion

        #region Methods
        private void LaunchToFarm(IPlayer client, double price = 0, Item item = null)
        {
            PlayerHandler ph = client.GetPlayerHandler();

            switch (Type)
            {
                case InteractionPointTypes.Farm:
                    Alt.Server.LogInfo("InteractionPoint | " + ph.PID + " a commence a farm " + _farm.Harvest_Name);
                    _farm?.StartFarming(client);
                    return;
                case InteractionPointTypes.DoubleProcess:
                    Alt.Server.LogInfo("InteractionPoint | " + ph.PID + " a commence a double process " + _farm.DoubleProcess_Name);
                    _farm?.StartDoubleProcessing(client);
                    return;
                case InteractionPointTypes.Process:
                    Alt.Server.LogInfo("InteractionPoint | " + ph.PID + " a commence a process " + _farm.Process_Name);
                    _farm?.StartProcessing(client);
                    return;
                case InteractionPointTypes.Sell:
                    Alt.Server.LogInfo("InteractionPoint | " + ph.PID + " a commence a vendre " + _farm.Selling_Name);
                    _farm?.StartSellingNew(client, price, item);
                    return;
                default:
                    Alt.Server.LogError("Problem, an unknwn Interactin type in InteractionPoints! ");
                    return;
            }
        }

        private void NpcInteractSell(IPlayer client, Ped npc)
        {
            if (Type != InteractionPointTypes.Sell || !client.Exists || client.IsInVehicle)
                return;

            PlayerHandler ph = client.GetPlayerHandler();

            foreach(KeyValuePair<double, Item> key in soldItems)
            {
                if (ph.CountItem(key.Value.id) <= 0)
                    continue;

                LaunchToFarm(client, key.Key, key.Value);
                return;
            }

            client.DisplayHelp("Vous n'avez rien à vendre!", 5000);
        }
        #endregion
    }
}
