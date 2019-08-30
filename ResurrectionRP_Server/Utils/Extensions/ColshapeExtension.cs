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
using MongoDB.Driver;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace ResurrectionRP_Server
{
    public static class ColshapeExtension
    {
        public static async void putPlayerInColshape(this IColShape colshape, IPlayer client)
        {
            if (!client.Exists && client.Type == BaseObjectType.Player)
                return;
            colshape.GetData<List<string>>("PlayersIn",out List<string> data);

            if ( data == null)
                 data = new List<string>();

            if (data.Exists(p => p == client.GetSocialClub()))
                return;

            data.Add(client.GetSocialClub());
            colshape.SetData("PlayersIn", data);
            colshape.GetData("PlayersIn", out List<string> test);
        }

        public static async void RemovePlayerInColshape(this IColShape colshape, IPlayer client)
        {
            if (!client.Exists && client.Type == BaseObjectType.Player)
                return;
            colshape.GetData<List<string>>("PlayersIn", out List<string> data);

            if (data == null)
            {
                colshape.SetData("PlayersIn", new List<string>());
                return;
            }

            colshape.GetData<List<string>>("PlayersIn", out data);
            if (data.Exists(p => p == client.GetSocialClub()))
                data.Remove(client.GetSocialClub());
            colshape.SetData("PlayersIn", data);
            colshape.GetData("PlayersIn", out List<string> test);

        }

        public static async Task<bool> IsPlayerInColshape(this IColShape colshape, IPlayer client)
        {
            if (!client.Exists && client.Type == BaseObjectType.Player)
                return false;
            colshape.GetData<List<string>>("PlayersIn", out List<string> data);
            if (data != null && data.Exists(p => client.GetSocialClub() == p))
                return true;
            return false;
        }

    }
}
