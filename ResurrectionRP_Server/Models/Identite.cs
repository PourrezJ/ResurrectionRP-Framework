using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AltV.Net;

namespace ResurrectionRP_Server.Models
{
    public class Identite
    {
        public Identite(string firstName, string lastName, string nationalite, DateTime birthDate)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Nationalite = nationalite;
            this.BirthDate = birthDate;
        }

        [JsonIgnore]
        public string Name
        {
            get => FirstName + " " + LastName;
        }

        public string FirstName;
        public string LastName;
        public int Age;
        public string Nationalite;
        public DateTime BirthDate;

        public static Identite GetOfflineIdentite(string socialClub)
        {
            try
            {
                var player = Entities.Players.PlayerManager.GetPlayerHandlerDatabase(socialClub).Result;
                return player?.Identite;
            }
            catch (Exception)
            {
                Alt.Server.LogError("Erreur : Identite.cs, ce problème est généralement dû au fait que le Handler du owner ne correspond pas à celui du nouveau sur Alt:V");
                return null;
            }
        }

    }
}
