using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Wine : Farm
    {
        public Wine()
        {
            Harvest_Name = "Vignoble";
            Process_Name = "Traitement du raisin";
            Selling_Name = "Revendeur de jus de raisin";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(-1802.502f, 2167.386f, 113.5449f);
            Harvest_Position.Add(new Vector3(-1802.502f, 2167.386f, 113.5449f));

            Process_PosRot = new Location(new Vector3(1545.689f, 2175.608f, 78.80177f), new Vector3(0, 0, 92.64625f));
            Selling_PosRot = new Location(new Vector3(-1553.488f, 1374.383f, 126.7967f), new Vector3(0, 0, 44.8825f));

            Harvest_Range = 50f;

            Process_PedHash = AltV.Net.Enums.PedModel.Dockwork01SMY;
            Selling_PedHash = AltV.Net.Enums.PedModel.Genstreet01AMO;

            BlipColor = (BlipColor)15;

            ItemIDBrute = ItemID.GrappeRaisin;
            ItemIDProcess = ItemID.GrapeJuice;
            ItemPrice = 75;
        }
    }
}