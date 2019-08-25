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

    public class BanManager
    {
        public List<Ban> BanList = new List<Ban>();

        public BanManager()
        {
        }

        public async Task Init()
        {
            var banList = await Database.MongoDB.GetCollectionSafe<Ban>("ban").AsQueryable().ToListAsync();
            foreach (Ban ban in banList)
            {
                BanList.Add(ban);

                await AltAsync.Do(() =>
                {
                    if (GameMode.Instance.PlayerList.Any(p => p.SocialClubId == ban.SocialID))
                    {
                        var player = GameMode.Instance.PlayerList.Find(p => p.Exists && (p.SocialClubId == ban.SocialID)) ?? null;
                        player.Kick("Vous êtes bannis du serveur.");
                    }


                });
            }
        }
        
        public static async Task BanPlayer(IPlayer player, string reason, DateTime endtime)
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

                GameMode.Instance.BanManager.BanList.Add(ban);
                Alt.Server.LogDebug ($"~grey~Ban du joueur {player.GetSocialClub()} pour la raison: {reason}");
                await Database.MongoDB.Insert("ban", ban);
                await player.KickAsync(reason);
                await Task.Delay(100);
            }

        } 
    }
}
