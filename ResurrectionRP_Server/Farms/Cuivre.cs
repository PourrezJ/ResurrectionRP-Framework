using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    class Cuivre : Farm
    {
        public Cuivre()
        {
            Harvest_Name = "Mine de cuivre";
            Process_Name = "Traitement de cuivre";
            Selling_Name = "Revendeur de cuivre";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(2684.724f, 2866.268f, 33f);
            Harvest_Position.Add(new Vector3(2684.724f, 2866.268f, 33f));

            Process_PosRot = new Location(new Vector3(741.797f, -972.2991f, 24.50507f), new Vector3(0f, 0f, -94.82858f));
            Selling_PosRot = new Location(new Vector3(606, -3073.102f, 6.06f), new Vector3(0, 0, -11.52882f));

            Harvest_Range = 50f;

            Process_PedHash = AltV.Net.Enums.PedModel.Dockwork01SMY;
            Selling_PedHash = AltV.Net.Enums.PedModel.Cntrybar01SMM;

            BlipColor = Entities.Blips.BlipColor.Complexion;

            ItemIDBrute = ItemID.MineraiCuivre;
            ItemIDProcess = ItemID.Cuivre;
            ItemPrice = 92;
        }
    }
}
