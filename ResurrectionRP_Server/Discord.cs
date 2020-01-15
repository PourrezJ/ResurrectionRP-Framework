using AltV.Net;
using AltV.Net.Elements.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ResurrectionRP_Server.Entities.Players.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public static class Discord
    {
        public static ConcurrentDictionary<IPlayer, DiscordData> DiscordPlayers;

        private static DiscordSocketClient _client;
        private static CommandService _commands;

        public static SocketGuild GTAVGuild { get; private set; }

        public static async Task Init()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig());
            DiscordPlayers = new ConcurrentDictionary<IPlayer, DiscordData>();

            //_client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            //_client.MessageReceived += MessageReceivedAsync;

            await _client.LoginAsync(TokenType.Bot, Config.GetSetting<string>("DiscordToken"));
            await _client.StartAsync();

        }

        private static Task ReadyAsync()
        {
            GTAVGuild = _client.GetGuild(Config.GetSetting<ulong>("GuildGTAV"));
            Alt.Server.LogInfo("Discord ready");
            return Task.CompletedTask;
        }

        public static SocketGuildUser GetSocketGuildUser(ulong id) => GTAVGuild.GetUser(id);

        public static bool HasRoleName(SocketGuildUser pdiscord, string name) => pdiscord.Roles.Any(p => p.Name.ToLower() == name.ToLower());

        public static bool IsAdmin(SocketGuildUser pdiscord) => HasRoleName(pdiscord, "Administrateur (NO-MP)");
        public static bool IsModerator(SocketGuildUser pdiscord) => HasRoleName(pdiscord, "modérateur");
        public static bool IsHelper(SocketGuildUser pdiscord) => HasRoleName(pdiscord, "helpers");
        public static bool IsAnimator(SocketGuildUser pdiscord) => HasRoleName(pdiscord, "animateur");
        public static bool IsCitoyen(SocketGuildUser pdiscord) => HasRoleName(pdiscord, "Citoyen");

        public static DiscordData GetDiscordData(IPlayer player)
        {
            if (DiscordPlayers.TryGetValue(player, out _))
                return DiscordPlayers[player];
            else return null;
        }

    }
}
