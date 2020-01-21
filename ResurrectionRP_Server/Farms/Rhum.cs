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
            Process_Name = "Distillerie (Rhum)";
            Selling_Name = "Revendeur de Rhum";
            //Selling_Name = "Revendeur de rhum";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;

            Harvest_BlipPosition = new Vector3(2262.616f, 4770.627f, 39.27166f);
            Harvest_Position.Add(new Vector3(2262.616f, 4770.627f, 39.27166f));

            //Process_PosRot = new Location(new Vector3(1255.333f, -2682.066f, 2.072282f), new Vector3(0, 0, 285.0722f));

            Harvest_Range = 10f;

            Process_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;

            BlipColor = (BlipColor)51;
          
            DoubleProcess_PosRot = new Location(new Vector3(2555.167f, 4651.451f, 34.07678f), new Vector3(0, 0, 0));
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