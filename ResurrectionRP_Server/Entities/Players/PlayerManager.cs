﻿using System;
using AltV.Net;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;
using AltV.Net.Async;
using System.Linq;

namespace ResurrectionRP_Server.Entities.Players
{
    public class PlayerManager
    {
        #region Variables 
        private static short Dimension = 2;
        public static int StartMoney = 0;
        public static int StartBankMoney = 0;

        private bool _whitelistOn = false;
        #endregion

        #region Constructor
        public PlayerManager()
        {
            Alt.OnClient("WhiteListCallback", WhiteListCallback);

            AltAsync.OnPlayerDead += Events_PlayerDeath;

            Utils.Utils.Delay(300000, false, async () =>
            {
                var players = PlayerHandler.PlayerHandlerList.ToList();

                for (int i = 0; i < players.Count; i++)
                {
                    var ph = players[i];

                    if (!ph.Key.Exists)
                        continue;

                    if (GameMode.Instance.PlayerList.Any(v => v.Id == ph.Key.Id))
                    {
                        if (ph.Value != null)
                            await ph.Value.UpdatePlayerInfo();
                    }
                    await Task.Delay(500);
                }

                players.Clear();
            });

        }



        /**private async Task OnKeyPress(object sender, Models.PlayerRemoteEventEventArgs eventArgs)
        {
            if (!eventArgs.Player.Exists)
                return;

            try
            {
                var ph = eventArgs.Player.GetPlayerHandler();

                if (ph != null && ph.OnKeyPressed != null)
                    await ph.OnKeyPressed.Invoke(eventArgs.Player, (ConsoleKey)eventArgs.Arguments[0]);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnKeyPress" + ex.Data);
            }
        }**/

        /**
        private async Task PlayerSync_TaskStartScenarioAtPosition(object sender, Models.PlayerRemoteEventEventArgs eventArgs)
        {
            if (!eventArgs.Player.Exists)
                return;

            var players = MP.Players.ToList();
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];

                if (player != null && player.Exists)
                {
                    await player.CallAsync("PlayerSyncCL_TaskStartScenarioAtPosition", eventArgs.Player.Id, eventArgs.Arguments[0], eventArgs.Arguments[1], eventArgs.Arguments[2], eventArgs.Arguments[3], eventArgs.Arguments[4], eventArgs.Arguments[5], eventArgs.Arguments[6], eventArgs.Arguments[7]);
                }
            }
        }

        private async Task Events_PlayerStreamIn(object sender, Models.PlayerStreamEventArgs eventArgs)
        {
            await eventArgs.ForPlayer.CallAsync("PlayerStream_DataUpdate", eventArgs.Player.Id, GetPlayerByClient(eventArgs.Player)?.PlayerSyncToJson);
        }**/
        #endregion

        #region ServerEvents

        private async Task Events_PlayerDeath(IPlayer player, IEntity killer, uint weapon)
        {
            if (!player.Exists)
                return;
            
            //if (GetPlayerByClient(player) != null)
                //await GetPlayerByClient(player)?.SetDead(true); // For fix client.Dead is doesn't work actually 
        }

        public async void WhiteListCallback(IPlayer player, object[] args)
        {
            string socialclub = args[0].ToString();
            if (await IsBan(player, socialclub))
            {
                await player.KickAsync("Vous êtes bannis!");
                return;
            }
            Models.Whitelist whitelist = await Models.Whitelist.GetWhitelistFromAPI(socialclub);
            
            if (whitelist != null && whitelist.Whitelisted)
            {
                if (whitelist.IsBan)
                {
                    if (DateTime.Now > whitelist.EndBanTime)
                    {
                        whitelist.IsBan = false;
                        await player.EmitAsync("OpenLogin", args[0]);
                        return;
                    }

                    string _kickMessage = $"Vous êtes ban du serveur jusqu'au {whitelist.EndBanTime.ToShortDateString()}";
                    await player.KickAsync(_kickMessage);
                }
                else
                {
                     player.Emit("OpenLogin", socialclub);
                }
            }
            else
            {
                await player.EmitAsync("FadeIn", 0);
                string _kickMessage = "Vous n'êtes pas whitelist sur le serveur";
                await player.KickAsync(_kickMessage);
            }
        }
        public async Task Events_PlayerJoin(IPlayer player, string reason)
        {

            if (!player.Exists)
                return;

            //await player.SetAlphaAsync(0);


            if (GameMode.ServerLock)
            {
                await player.EmitAsync("fadeIn", 0);
                await player.KickAsync("Serveur Lock!");
            }

            Alt.Server.LogInfo($" {player.SocialClubId} ({player.Ip}) en attente de connexion.");
            while (!GameMode.Instance.ServerLoaded)
                await Task.Delay(100);


            await player.SetDimensionAsync(Dimension++);
            if (!GameMode.Instance.IsDebug || 1==1)
            {
                try
                {
                    
                    if (!Config.GetSetting<bool>("WhitelistOpen"))
                    {
                        await player.EmitAsync("OpenLogin");

                        return;
                    }
                    player.Emit("GetSocialClub", "WhiteListCallback");
                }
                catch (Exception ex)
                {
                    string _kickMessage = "Vous n'êtes pas whitelist sur le serveur";
                    await player.KickAsync(_kickMessage);
                    Alt.Server.LogError("Player Login" + ex.Data);
                }
            }
            else
            {
                //await ConnectPlayer(player);
            }
        }
        #endregion

        #region Methods 
        public static async Task<PlayerHandler> GetPlayerHandlerDatabase(string socialClub) =>
            await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();


        public static PlayerHandler GetPlayerByClient(IPlayer player)
        {
            if (!player.Exists)
                return null;

            if (player.GetData("PlayerHandler", out object data))
            {
                return data as PlayerHandler;
            }
            else if (PlayerHandler.PlayerHandlerList.TryGetValue(player, out PlayerHandler value))
            {
                return value;
            }
            return null;
        }

        public static async Task<bool> IsBan(IPlayer player, string social)
        {
            if (GameMode.Instance.BanManager == null)
                return false;

            for (int a = 0; a < GameMode.Instance.BanManager.BanList.Count; a++)
            {
                if (GameMode.Instance.BanManager.BanList[a].SocialClub == social)
                    return true;

            }

            return false;
        }

        #endregion

    }
}
