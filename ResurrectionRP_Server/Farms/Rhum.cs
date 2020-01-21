using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Rhum : Farm
    {
        public Rhum()
        {
            Harvest_Name = "Champ de canne a sucre";
            DoubleProcess_Name = "Distillerie (Rhum)";
            Selling_Name = "Revendeur de Rhum";


            Harvest_BlipSprite = 85;
            DoubleProcess_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(2262.616f, 4770.627f, 39.27166f);
            Harvest_Position.Add(new Vector3(2262.616f, 4770.627f, 39.27166f));

            Harvest_Range = 10f;

            DoubleProcess_Time = 40000;

            BlipColor = (BlipColor)51;
          
            DoubleProcess_PosRot = new Location(new Vector3(346.53625f, 3405.9297f, 36.457886f), new Vector3(0, 0, 0));
            Selling_PosRot = new Location(new Vector3(-1256.917f, -1149.656f, 7.604019f), new Vector3(0, 0, 246.4353f));

            DoubleProcess_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;
            Selling_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;

            BlipColor = (BlipColor)51;

            ItemIDBrute = ItemID.Canneasurcre;
            ItemIDBrute2 = ItemID.BouteilleTraite;
            ItemIDProcess = ItemID.Rhum;

            ItemPrice = 481;
        }
    }
}