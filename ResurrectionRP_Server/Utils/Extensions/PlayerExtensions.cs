using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using System;

namespace ResurrectionRP_Server.Utils.Extensions
{
    public static class PlayerExtensions
    {
        public static string GetSocialClub(this IPlayer player)
        {
            if (player.GetData<string>("SocialClub", out string data))
                return data;
            throw new NullReferenceException("SocialClubMising");
        }

        public static string GetTeamspeakID(this IPlayer player)
        {
            var ID = "NothingAtAll";
            if (player.GetData<string>("PLAYER_TEAMSPEAK_IDENT", out string data))
                ID = data;
            return ID;
        }

        public static ushort GetTeamspeakClientID(this IPlayer player)
        {
            ushort ID = 0;
            if (player.GetData("VOICE_TS_ID", out ushort data))
                ID = data;
            return ID;
        }

        public static PlayerHandler GetPlayerHandler(this IPlayer client)
        {
            if (client == null)
                return null;

            if (!client.Exists)
                return null;

            if (PlayerHandler.PlayerHandlerList.TryGetValue(client, out PlayerHandler value)) return value;
            return null;
        }
    }
}
