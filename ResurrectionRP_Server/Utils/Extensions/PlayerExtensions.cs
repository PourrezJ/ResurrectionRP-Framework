using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using ResurrectionRP_Server.Entities.Players;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using AltV.Net.Data;
using AltV.Net.Elements.Args;

namespace ResurrectionRP_Server
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

        public async static Task SetClothAsync(this IPlayer client, Models.ClothSlot level, int drawable, int texture, int palette) =>
            await client.EmitAsync("ComponentVariation", ((int)level), drawable, texture, palette);

        public async static Task SetPropAsync(this IPlayer client, Models.PropSlot slot, Models.PropData item) =>
            await client.EmitAsync("PropVariation", (int)slot, item.Drawable, item.Texture);

        public static PlayerHandler GetPlayerHandler(this IPlayer client)
        {
            if (client == null)
                return null;

            if (!client.Exists)
                return null;

            if (PlayerHandler.PlayerHandlerList.TryGetValue(client, out PlayerHandler value)) return value;
            return null;
        }

        public static async Task NotifyAsync(this IPlayer player, string text)
        {

        }

    }
}
