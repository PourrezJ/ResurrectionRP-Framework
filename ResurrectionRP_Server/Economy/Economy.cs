using System;

namespace ResurrectionRP_Server.Economy
{
    public class Economy
    {
        // Taxes Variable
        public double Taxe_Market = 1.0;
        public double Taxe_Essence = 1.0;
        public double Taxe_Exportation = 1.0;

        // Price
        public int Price_Parking = 200;
        public int Price_Soins = 500;
        public int Price_RespawnSoins = 1000;
        public int Price_Fourriere = 3000;

        // Caisse
        public double CaissePublique;

        public static double CalculPriceTaxe(double Price, double Taxes)
        {
            double _price = Price * (Taxes / 100);
            return _price;
        }

        public static double CalculNewPrice(double Price, double Taxes)
        {
            double _price = Price + Taxes;
            return _price;
        }

        public static decimal CustomRound(decimal num) => Math.Round(num * 20.0M, MidpointRounding.AwayFromZero) / 20.0M;
    }
}
