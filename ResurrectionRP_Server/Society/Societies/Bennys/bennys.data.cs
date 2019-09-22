using System;
using System.Collections.Generic;
using System.Linq;


namespace ResurrectionRP_Server.Society.Societies.Bennys
{
    public enum GarageType
    {
        Car,
        Bike
    }

    public class BennysData
    {
        public struct EsthetiqueData
        {
            public readonly byte ModID;
            public readonly string ModName;
            public readonly double ModPrice;

            public EsthetiqueData(byte modID, string modName, double modPrice)
            {
                ModID = modID;
                ModName = modName;
                ModPrice = modPrice;
            }
        }

        public struct PerformanceData
        {
            public readonly byte ModID;
            public readonly string ModName;
            public readonly double[] ModPrice;

            public PerformanceData(byte modID, string modName, double[] modPrice)
            {
                ModID = modID;
                ModName = modName;
                ModPrice = modPrice;
            }
        }

        public static EsthetiqueData[] EsthetiqueModList = new EsthetiqueData[]
        {
            new EsthetiqueData(0, "Spoiler", 2000),
            new EsthetiqueData(1, "Pare-choc Avant", 2000),
            new EsthetiqueData(2, "Pare-choc Arrière", 2000),
            new EsthetiqueData(3, "Jupe latérale", 2000),
            new EsthetiqueData(4, "Échappement", 2000),
            new EsthetiqueData(5, "Cadre", 2000),
            new EsthetiqueData(6, "Grille", 2000),
            new EsthetiqueData(7, "Capot", 2000),
            new EsthetiqueData(8, "Aile", 2000),
            new EsthetiqueData(9, "Aile Droite", 2000),
            new EsthetiqueData(10, "Toit", 2000),
            new EsthetiqueData(14, "Klaxon", 2000),
            new EsthetiqueData(22, "Xenon", 2000),
            new EsthetiqueData(23, "Roues", 2000),
            new EsthetiqueData(25, "Supports de plaque", 2000),
            new EsthetiqueData(27, "Design intérieur", 2000),
            new EsthetiqueData(28, "Ornements", 2000),
            new EsthetiqueData(30, "Compteur", 2000),
            new EsthetiqueData(33, "Volant", 2000),
            new EsthetiqueData(34, "Levier de vitesses", 2000),
            new EsthetiqueData(35, "Plaques", 2000),
            new EsthetiqueData(38, "Hydraulics", 2000),
            new EsthetiqueData(46, "Teinte des vitres", 2000),
            new EsthetiqueData(48, "Skin", 2000),
            new EsthetiqueData(62, "Assiette", 2000),
            new EsthetiqueData(69, "Teinte des vitres", 2000),
            new EsthetiqueData(74, "Couleur du tableau de bord", 2000),
            new EsthetiqueData(75, "Couleur de garniture", 2000)
        };

        public static EsthetiqueData? GetEsthetiqueData(int modID)
        {
            for (int i = 0; i < EsthetiqueModList.Count(); i++)
            {
                if (EsthetiqueModList[i].ModID == modID)
                    return EsthetiqueModList[i];
            }
            return null;
        }

        public static PerformanceData? GetPerformanceData(int modID)
        {
            for (int i = 0; i < PerformanceModList.Count(); i++)
            {
                if (PerformanceModList[i].ModID == modID)
                    return PerformanceModList[i];
            }
            return null;
        }

        public static PerformanceData[] PerformanceModList = new PerformanceData[]
        {
            new PerformanceData(11, "Moteur", new double[] { 0, 7000, 14000, 21000, 35000, 50000}),
            new PerformanceData(12, "Frein", new double[] { 0, 3500, 7000, 10500}),
            new PerformanceData(13, "Transmission", new double[] { 0, 5250, 10500, 15750}),
            new PerformanceData(15, "Suspensions", new double[] { 0, 1750, 2625, 3500, 5250}),
            //new PerformanceData(18, "Turbo", new double[] { 0, 78250 })
        };

        public static double CalculPrice(Entities.Vehicles.VehicleHandler vehicle, double originalPrice)
        {
            var multiplicator = 0;
            double price = 0;

            switch (vehicle.VehicleManifest.VehicleClass)
            {
                case 0: // Compact
                    multiplicator = 155;
                    break;

                case 1: // Sedans
                    multiplicator = 77;
                    break;

                case 2: // SUVs
                    multiplicator = 285;
                    break;

                case 3: // Coupes
                    multiplicator = 285;
                    break;

                case 4: // Muscle
                    multiplicator = 285;
                    break;

                case 5: // Sports Classics
                    multiplicator = 800;
                    break;

                case 6: // Sports
                    multiplicator = 800;
                    break;

                case 7: // Super
                    multiplicator = 2000;
                    break;

                case 9: // Off-Road
                    multiplicator = 165;
                    break;

                case 12: // Vans
                    multiplicator = 165;
                    break;
            }

            price = originalPrice * (multiplicator / 100);

            if (price == 0)
                return originalPrice;

            return price;
        }
    }
}
