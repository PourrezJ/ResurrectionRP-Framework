using AltV.Net.Async;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net;

namespace ResurrectionRP_Server.Models
{
    public class Ban
    {
        [BsonId]
        public string SocialClub;
        public ulong SocialID;
        public DateTime DateEnd;

        public Ban()
        {

        }
    }

    public static class BanManager
    {
        public static List<Ban> BanList = new List<Ban>();

        public static void Init()
        {
            var banList = Database.MongoDB.GetCollectionSafe<Ban>("ban").AsQueryable(); ;
            foreach (Ban ban in banList)
            {
                BanList.Add(ban);

                if (GameMode.PlayerList.Any(p => p.SocialClubId == ban.SocialID))
                {
                    var player = GameMode.PlayerList.Find(p => p.Exists && (p.SocialClubId == ban.SocialID)) ?? null;
                    player.Kick("Vous êtes bannis du serveur.");
                }
            }
        }
        
        public static void BanPlayer(IPlayer player, string reason, DateTime endtime)
        {
            var players = Entities.Players.PlayerHandler.PlayerHandlerList.ToList();

            for (int i = 0; i < players.Count; i++)
            {
                var ban = new Ban()
                {
                    SocialClub = player.GetSocialClub(),
                    SocialID = player.SocialClubId,
                    DateEnd = endtime
                };

                BanList.Add(ban);
                Alt.Server.LogColored($"~c~Ban du joueur {player.GetSocialClub()} pour la raison: {reason}");
                player.Kick(reason);
                Task.Run(async () => await Database.MongoDB.Insert("ban", ban));
            }
        } 
    }
}
