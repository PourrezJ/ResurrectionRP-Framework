using ResurrectionRP_Server.Models;
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

        public Bourse Bourse = new Bourse();

        public static double CalculPriceTaxe(double Price, double Taxes)
        {
            return Math.Round(Price * (Taxes / 100), 3);
        }

        public static double CalculNewPrice(double Price, double Taxes)
        {
            return Price + Taxes;
        }

        public static decimal CustomRound(decimal num) => Math.Round(num * 20.0M, MidpointRounding.AwayFromZero) / 20.0M;
    }
}
