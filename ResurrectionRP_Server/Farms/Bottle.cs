using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Bottle : Farm
    {
        public Bottle()
        {
            Process_Name = "Stérilisation de bouteilles";
            Selling_Name = "Revendeur de bouteilles stérilisées";

            Process_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Process_PosRot = new Location(new Vector3(2889.869f, 4391.395f, 50.45136f), new Vector3(0, 0, 294.4743f));
            Selling_PosRot = new Location(new Vector3(274.8265f, -3015.225f, 5.698002f), new Vector3(0, 0, 97.31085f));

            Process_PedHash = PedModel.GarbageSMY;
            Selling_PedHash = PedModel.BoatStaff01F;

            BlipColor = (BlipColor)73;

            Process_QuantityNeeded = 4;
            Process_Time = 50000;
            ItemIDBrute = ItemID.Sable;
            ItemIDProcess = ItemID.BouteilleTraite;
            ItemPrice = 248;
        }
    }
}
