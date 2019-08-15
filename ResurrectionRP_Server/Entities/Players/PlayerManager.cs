using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;
using AltV.Net.Async;
using AltV.Net.Async.Events;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using WordPressPCL;
using WordPressPCL.Models;

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

            Alt.OnClient("SendLogin", SendLogin );
            Alt.OnClient("LogPlayer", LogPlayer);
            Alt.OnClient("Events_PlayerJoin", Events_PlayerJoin);
            Alt.OnClient("UpdateHungerThirst", UpdateHungerThirst);

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

        public async void Events_PlayerJoin(IPlayer player, object[] args)
        {

            if (!player.Exists)
                return;

            string socialclub = args[0].ToString();

            player.SetData("SocialClub", socialclub);
            player.Model = (uint)AltV.Net.Enums.PedModel.FreemodeMale01;
            player.Spawn(new Vector3(-1072.886f, -2729.607f, 0.8148939f), 0);

            if (GameMode.ServerLock)
            {
                await player.EmitAsync("FadeIn", 0);
                await player.KickAsync("Serveur Lock!");
            }

            Alt.Server.LogInfo($" {player.SocialClubId} ({player.Ip}) en attente de connexion.");
            while (!GameMode.Instance.ServerLoaded)
                await Task.Delay(100);

            await player.SetDimensionAsync(Dimension++);
            if (!GameMode.Instance.IsDebug)
            {
                try
                {
                    
                    if (!Config.GetSetting<bool>("WhitelistOpen"))
                    {
                        await player.EmitAsync("OpenLogin");

                        return;
                    }
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
                catch (Exception ex)
                {
                    string _kickMessage = "Vous n'êtes pas whitelist sur le serveur";
                    await player.KickAsync(_kickMessage);
                    Alt.Server.LogError("Player Login" + ex.Data);
                }
            }
            else
            {
                await ConnectPlayer(player);
            }
        }
        #endregion

        #region RemoteEvents

        private async void SendLogin(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var definition = new { login = "", password = "", socialClub = "" };

            var data = JsonConvert.DeserializeAnonymousType(args[0].ToString(), definition);
            var wpclient = new WordPressClient("https://resurrectionrp.fr/wp-json/");
            wpclient.AuthMethod = AuthMethod.JWT;

            try
            {
                await wpclient.RequestJWToken(data.login, data.password);
                if (await wpclient.IsValidJWToken())
                {
                    await client.EmitAsync("LoginOK", await PlayerHandlerExist(client));
                }
                else
                {
                    await client.EmitAsync("LoginError", "");
                }
            }
            catch
            {
                await client.EmitAsync("LoginError", "");
                return;
            }
        }
        private async void UpdateHungerThirst(IPlayer client, object[] arg)
        {
            if (!client.Exists)
                return;

            PlayerHandler ph = GetPlayerByClient(client);
            if (ph != null)
            {
                ph.Hunger = Convert.ToInt32(arg[0]);
                ph.Thirst = Convert.ToInt32(arg[1]);
                await ph.UpdatePlayerInfo();
            }
        }
        private async void LogPlayer(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            await ConnectPlayer(client);
        }
        public static async Task ConnectPlayer(IPlayer client)
        {
            if (await PlayerHandlerExist(client))
            {
                await client.EmitAsync("FadeOut",0);
                client.GetData("SocialClub", out string social);
                PlayerHandler player = await GetPlayerHandlerDatabase( social );
                player.LastUpdate = DateTime.Now;
                await player.LoadPlayer(client);
            }
            else
            {
                Alt.Log("LE CREATEUR N'EST PAS ENCORE FAIT OMG");
                //await OpenCreator(client);
            }
        }

        #endregion

        #region Methods 
        public static async Task<PlayerHandler> GetPlayerHandlerDatabase(string socialClub) =>
            await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();

        public static async Task<bool> PlayerHandlerExist(IPlayer player)
        {
            try
            {
                player.GetData("SocialClub", out string social);
                
                return await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == social.ToLower()).AnyAsync();
            }
            catch (Exception ex)
            {
                //await player.SendNotificationError("Erreur avec votre compte, contactez un membre du staff.");
                Alt.Server.LogError("PlayerHandlerExist" + ex.Data);
                Alt.Server.LogError("PlayerHandlerExist" + ex.Data);
            }
            return false;
        }

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
