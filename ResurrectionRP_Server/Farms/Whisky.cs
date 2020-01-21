using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ResurrectionRP_Server.Farms
{
    public class Whisky : Farm
    {
        public Whisky()
        {
            Harvest_Name = "Champ d'orge";
            DoubleProcess_Name = "Distillerie (Whisky)";
            Selling_Name = "Revendeur de Whisky";

            Harvest_BlipSprite = 85;
            DoubleProcess_BlipSprite = 499;
            Selling_BlipSprite = 500;

            Harvest_BlipPosition = new Vector3(2225.7495f, 5044.101f, 45.472534f);
            Harvest_Position.Add(new Vector3(2225.7495f, 5044.101f, 45.472534f));

            Harvest_Range = 10f;

            DoubleProcess_Time = 40000;

            BlipColor = BlipColor.Snuff;

            DoubleProcess_PosRot = new Location(new Vector3(342.03955f, 3404.5188f, 36.491577f), new Vector3(0, 0, 0));
            Selling_PosRot = new Location(new Vector3(245.3934f, 370.4967f, 105.72742f), new Vector3(0, 0, -2.8694863f));

            DoubleProcess_PedHash = AltV.Net.Enums.PedModel.Chef;
            Selling_PedHash = AltV.Net.Enums.PedModel.Chef;

            BlipColor = BlipColor.Snuff;

            ItemIDBrute = ItemID.Orge;
            ItemIDBrute2 = ItemID.BouteilleTraite;
            ItemIDProcess = ItemID.Whisky;

            ItemPrice = 461;
        }
    }
}
