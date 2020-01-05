using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class GasJerrycan : Item
    {
        public double Fuel { get; set; }
        public GasJerrycan(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "gasjerrycan", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Fuel = 0;
        }

        public override void Use(IPlayer client, string inventoryType, int slot)
        {
            if (!client.Exists)
                return;

            PlayerHandler _client = client.GetPlayerHandler();
            GasJerrycan item = null;

            switch (inventoryType)
            {
                case Utils.Enums.InventoryTypes.Pocket:
                    item = _client.PocketInventory.InventoryList[slot].Item as GasJerrycan;
                    break;

                case Utils.Enums.InventoryTypes.Bag:
                    item = _client.BagInventory.InventoryList[slot].Item as GasJerrycan;
                    break;

                case Utils.Enums.InventoryTypes.Distant:
                    client.DisplayHelp("Le jerrycan doit être sur vous !", 5000);
                    return;
            }

            if (client.Vehicle != null)
            {
                client.DisplayHelp("Vous ne pouvez être dans un véhicule pour faire ça.", 10000);
                return;
            }
            
            VehicleHandler _vehicle = client.GetNearestVehicle(5)?.GetVehicleHandler();
            if (_vehicle != null)
            {
                if (item.Fuel == 0)
                {
                    client.SendNotificationError("Votre jerrycan est vide");
                    return;
                }
                    
                if(_vehicle.VehicleData.Fuel == _vehicle.VehicleData.FuelMax || _vehicle.VehicleData.FuelMax - _vehicle.VehicleData.Fuel < 2)
                {
                    client.SendNotificationError("Inutile de remplir un véhicule plein.");
                    return;
                }
                if((_vehicle.VehicleData.Fuel + (float)item.Fuel) >= _vehicle.VehicleData.FuelMax)
                {
                    item.Fuel -= Math.Round(_vehicle.VehicleData.FuelMax - _vehicle.VehicleData.Fuel, 2);
                    client.SendNotificationSuccess("Vous avez mis " + Math.Round(_vehicle.VehicleData.FuelMax - _vehicle.VehicleData.Fuel, 2) + "L d'essence.<br/>Il reste " + Math.Round(item.Fuel , 2)+ "L dans le jerrycan");
                    _vehicle.VehicleData.Fuel = _vehicle.VehicleData.FuelMax;
                } else
                {
                    _vehicle.VehicleData.Fuel += (float)item.Fuel;
                    client.SendNotificationSuccess("Vous avez mis " + (item.Fuel) + "L d'essence.<br/>Votre jerrycan est maintenant vide");
                    item.Fuel = 0;
                }
            }
        }

    }
}
