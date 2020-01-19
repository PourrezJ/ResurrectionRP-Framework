using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models
{
    public class Whitelist
    {
        [BsonId]
        public string _id { get; set; }
        public string Email { get; set; }
        public bool Whitelisted { get; set; }
        public bool IsBan { get; set; }
        public bool NeedVoice { get; set; }
        public DateTime EndBanTime { get; set; }
        public Formulaire Formulaire { get; set; }
        public string IP { get; set; }
        public string SocialId { get; set; }

        public static async Task<Whitelist> GetWhitelistFromAPI(string social)
        {
            string data = "";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"http://api.resurrectionrp.fr/api/whitelist/get?user={social}");
                if (response.IsSuccessStatusCode)
                {
                    data = await response.Content.ReadAsStringAsync();
                }
            }
            return JsonConvert.DeserializeObject<Whitelist>(data);
        }
    }

    public class Formulaire
    {
        public string discordName { get; set; }
        public string socialClub { get; set; }
        public string email { get; set; }
        public string age { get; set; }
        public string experienceRp { get; set; }
        public string sourceResu { get; set; }
        public string definitionRoleplay { get; set; }
        public string rpName { get; set; }
        public string histoireRp { get; set; }
        public string qualitesDefauts { get; set; }
        public string souvenirsBackground { get; set; }
        public string pourquoiRejoindre { get; set; }
        public string objectifsRp { get; set; }
        public string contributions { get; set; }
        public string hrp { get; set; }
        public string argentRp { get; set; }
        public string controlePolice { get; set; }
        public string braquageRp { get; set; }
        public string commentaire { get; set; }
    }
}
