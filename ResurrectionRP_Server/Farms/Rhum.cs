﻿using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Models.InventoryData;
using System.Numerics;

namespace ResurrectionRP_Server.Farms
{
    public class Rhum : Farm
    {
        public Rhum()
        {
            Harvest_Name = "Champ de canne a sucre";
            Process_Name = "Distillerie";
            //Selling_Name = "Revendeur de rhum";

            Harvest_BlipSprite = 85;
            Process_BlipSprite = 499;

            Harvest_BlipPosition = new Vector3(2262.616f, 4770.627f, 39.27166f);
            Harvest_Position.Add(new Vector3(2262.616f, 4770.627f, 39.27166f));

            Harvest_Range = 10f;

            Process_PedHash = AltV.Net.Enums.PedModel.Beach03AMY;

            BlipColor = (BlipColor)51;

            ItemIDBrute = ItemID.Canneasurcre;
            ItemIDProcess = ItemID.RhumLiquide;
        }
    }
}