using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Sable : Farm
    {
        public Sable()
        {
            Harvest_Name = "Sable";
            Process_Name = "Traitement de verre";
            Selling_Name = "Revendeur de bouteilles";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(-2758.732f, 2730.944f, 1.734188f);
            Harvest_Position.Add(new Vector3(-2758.732f, 2730.944f, 1.734188f));

            Process_PosRot = new Location(new Vector3(974.3141f, -1937.197f, 32.22253f), new Vector3(0, 0, 101.5686f));
            Selling_PosRot = new Location(new Vector3(270.2509f, -3075.726f, 5.774549f), new Vector3(0, 0, 215.7791f));

            Process_PedHash = AltV.Net.Enums.PedModel.Floyd;
            Selling_PedHash = AltV.Net.Enums.PedModel.BoatStaff01F;

            Harvest_Range = 50f;

            BlipColor = Entities.Blips.BlipColor.Yellow;

            ItemIDBrute = ItemID.Sable;
            ItemIDProcess = ItemID.Bouteille;
            ItemPrice = 98;
        }
    }
}
