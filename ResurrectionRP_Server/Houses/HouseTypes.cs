using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Houses
{
    #region HouseType Class
    public class HouseType
    {
        public string Name { get; }
        public Location Position { get; }
        public int InventorySize { get; }
        public int ParkingPlace { get; }

        public HouseType(string name, Location position, int inventorySize, int parkingPlace = 0)
        {
            Name = name;
            Position = position;
            InventorySize = inventorySize;
            ParkingPlace = parkingPlace;
        }
    }
    #endregion

    public class HouseTypes
    {
        public static List<HouseType> HouseTypeList = new List<HouseType>
        {
            //https://docs.google.com/spreadsheets/d/1lLVWXIVNJLVo5IGa1cOmX28awnoTb_uXoknQaQEY02M/edit#gid=0
            // name, position
            new HouseType("Labo de Weed", new Location(new Vector3(0,0,0), new Vector3()), 5000), // 0
            new HouseType("Un Bureau",  new Location(new Vector3(1165f, -3196.6f, -39.01306f), new Vector3()), 750), // 1

            new HouseType("Caravane", new Location(new Vector3(1972.8f, 3816.627f, 33.42873f), new Vector3()), 150, 1), // 2 250 000
            new HouseType("Bas de gamme", new Location(new Vector3(266.0099f, -1007.355f, -101.0086f), new Vector3()), 200, 2), // 3
            new HouseType("Moyenne gamme", new Location(new Vector3(346.6649f, -1012.871f, -99.19622f), new Vector3(0, 0, 358.6253f)), 250 , 3), // 4
            new HouseType("4 Integrity Way, Apt 28", new Location(new Vector3(-18.07856f, -583.6725f, 79.46569f), new Vector3()), 350),
            new HouseType("4 Integrity Way, Apt 30", new Location(new Vector3(-35.31277f, -580.4199f, 88.71221f), new Vector3()), 350),
            new HouseType("Dell Perro Heights, Apt 4", new Location(new Vector3(-1468.14f, -541.815f, 73.4442f), new Vector3()), 350),
            new HouseType("Dell Perro Heights, Apt 7",new Location(new Vector3(-1477.14f, -538.7499f, 55.5264f), new Vector3()), 350),
            new HouseType("Richard Majestic, Apt 2", new Location(new Vector3(-915.811f, -379.432f, 113.6748f), new Vector3()), 350),
            new HouseType("Tinsel Towers, Apt 42", new Location(new Vector3(-614.86f, 40.6783f, 97.60007f), new Vector3()), 350),
            new HouseType("Eclipse Towers, Apt 3", new Location(new Vector3(-773.407f, 341.766f, 211.397f), new Vector3()), 350),
            new HouseType("3655 Wild Oats Drive", new Location(new Vector3(-169.286f, 486.4938f, 137.4436f), new Vector3()), 350),
            new HouseType("2044 North Conker Avenue", new Location(new Vector3(340.9412f, 437.1798f, 149.3925f), new Vector3()), 350),
            new HouseType("2045 North Conker Avenue", new Location(new Vector3(373.023f, 416.105f, 145.7006f), new Vector3()), 350),
            new HouseType("2862 Hillcrest Avenue", new Location(new Vector3(-676.127f, 588.612f, 145.1698f), new Vector3()), 350),
            new HouseType("2868 Hillcrest Avenue", new Location(new Vector3(-763.107f, 615.906f, 144.1401f), new Vector3()), 350),
            new HouseType("2874 Hillcrest Avenue", new Location(new Vector3(-857.798f, 682.563f, 152.6529f), new Vector3()), 350),
            new HouseType("2677 Whispymound Drive", new Location(new Vector3(120.5f, 549.952f, 184.097f), new Vector3()), 350),
            new HouseType("2133 Mad Wayne Thunder", new Location(new Vector3(-1288f, 440.748f, 97.69459f), new Vector3()), 350),
        };
    }
}