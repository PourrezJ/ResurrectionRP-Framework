using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    class RhumBottle : Farm
    {
        public RhumBottle()
        {
            DoubleProcess_Name = "Mise en bouteille (Rhum)";
            Selling_Name = "Revendeur de Rhum";

            DoubleProcess_BlipSprite = 499;
            Selling_BlipSprite = 500;

            DoubleProcess_PosRot = new Location(new Vector3(2555.167f, 4651.451f, 34.07678f), new Vector3(0, 0, 100.0674f));
            Selling_PosRot = new Location(new Vector3(-1256.917f, -1149.656f, 7.604019f), new Vector3(0, 0, 246.4353f));

            DoubleProcess_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;
            Selling_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;

            BlipColor = (BlipColor)51;

            ItemIDBrute = ItemID.RhumLiquide;
            ItemIDBrute2 = ItemID.BouteilleTraite;
            ItemIDProcess = ItemID.Rhum;

            ItemPrice = 481;
        }
    }
}