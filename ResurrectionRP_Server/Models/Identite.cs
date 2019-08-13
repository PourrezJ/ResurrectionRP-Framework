using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

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

        public async static Task<Identite> GetOfflineIdentite(string socialClub)
        {
            
            var player = await Entities.Players.PlayerManager.GetPlayerHandlerDatabase(socialClub);
            return player?.Identite;
        }

    }
}
