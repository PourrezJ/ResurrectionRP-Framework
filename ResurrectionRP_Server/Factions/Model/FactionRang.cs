using Newtonsoft.Json;
using System;

namespace ResurrectionRP_Server.Factions.Model
{
    public class FactionRang
    {
        [JsonProperty("FactionRang")]
        public string RangName { get; set; }
        public int Rang { get; set; }
        public bool Recrute { get; set; }
        public double Salaire { get; set; }
        public bool CanTakeMoney { get; set; }
        public bool CanDepositMoney { get; set; }
        public DateTime LastRecept { get; set; }

        public FactionRang(int rang, string rangName, bool recrute, double salaire = 0, bool moneyGestion = false, bool canDeposit = true)
        {
            RangName = rangName;
            Rang = rang;
            Recrute = recrute;
            Salaire = salaire;
            CanTakeMoney = moneyGestion;
            CanDepositMoney = canDeposit;
        }
    }
}
