using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public class DiscordData
    {
        public string id;
        public string name;
        public string discriminator;
        public string avatar;

        public SocketGuildUser SocketGuildUser;
    }
}