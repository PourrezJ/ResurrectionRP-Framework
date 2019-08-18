using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using ResurrectionRP_Server.Entities.Players;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;

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

        public static PlayerHandler GetPlayerHandler(this IPlayer client)
        {
            if (client == null)
                return null;

            if (!client.Exists)
                return null;

            if (PlayerHandler.PlayerHandlerList.TryGetValue(client, out PlayerHandler value)) return value;
            return null;
        }

        public async static Task SendNotification(this IPlayer client, string text)
        {

        }
        public async static Task SendNotificationError(this IPlayer client, string text)
        {

        }
        public async static Task SendNotificationSuccess(this IPlayer client, string text)
        {

        }

        public async static Task NotifyAsync(this IPlayer client, string text)
        {

        }

        public static List<IVehicle> GetVehiclesInRange(this IPlayer client, int Range)
        {
            var vehs = Alt.GetAllVehicles();
            List<IVehicle> endup = new List<IVehicle>();
            var position = client.GetPosition();
            Vector3 osition = new Vector3(position.X, position.Y, position.Z);
            foreach(IVehicle veh in vehs)
            {
                if (!veh.Exists)
                    continue;
                var vehpos = veh.GetPosition();
                if(osition.DistanceTo2D(new Vector3(vehpos.X, vehpos.Y, vehpos.Z)) <= Range)
                {
                    endup.Add(veh);
                }
            }
            return endup;
        }
    }
}
