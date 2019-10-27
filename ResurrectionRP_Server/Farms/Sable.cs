using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Sable : Farm
    {
        private static int UsureOutil = 1;
        public Sable()
        {
            NewFarm = true;
            Harvest_Name = "Sable";
            Process_Name = "Traitement de verre";
            Selling_Name = "Revendeur de bouteilles";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(-788.2813f, 5868.633f, 6.380f);
            Harvest_Position.Add(new Vector3(-756.78f, 5876.49f, 8.8f));
            Harvest_Position.Add(new Vector3(-774.936f, 5875.556f, 7.791f));
            Harvest_Position.Add(new Vector3(-794.6f, 5874.446f, 6.134f));
            Harvest_Position.Add(new Vector3(-791.688f, 5864.644f, 5.551f));
            Harvest_Position.Add(new Vector3(-772.354f, 5867.396f, 8.084f));
            Harvest_Position.Add(new Vector3(-751.615f, 5868.162f, 9.774f));
            Harvest_Position.ForEach((position) =>
                FarmPoints.Add(new InteractionPoint(this, position, 0, new List<Item> { Inventory.Inventory.ItemByID(ItemID.Pioche), Inventory.Inventory.ItemByID(ItemID.MarteauPiqueur) }, InteractionPointTypes.Farm, "miner", 0))
            );

            Process_PosRot = new Location(new Vector3(974.3141f, -1937.197f, 32.22253f), new Vector3(0, 0, 101.5686f));
            Selling_PosRot = new Location(new Vector3(270.2509f, -3075.726f, 5.774549f), new Vector3(0, 0, 215.7791f));

            Process_PedHash = AltV.Net.Enums.PedModel.Floyd;
            Selling_PedHash = AltV.Net.Enums.PedModel.BoatStaff01F;

            Harvest_Range = 50f;

            BlipColor = Entities.Blips.BlipColor.Yellow;

            ItemIDBrute = ItemID.Sable;
            ItemIDProcess = ItemID.Bouteille;
            ItemPrice = 98;


            Harvest_Time = 5000;
            Process_Time = 5000;
            DoubleProcess_Time = 5000;
        }
    }
}
