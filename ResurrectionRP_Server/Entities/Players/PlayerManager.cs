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
using ResurrectionRP_Server.Utils.Extensions;

namespace ResurrectionRP_Server.Entities.Players
{
    public class PlayerManager
    {
        #region Variables 
        private static short Dimension = short.MaxValue;
        public static int StartMoney = 0;
        public static int StartBankMoney = 0;

        private bool _whitelistOn = false;
        #endregion

        #region Constructor
        public PlayerManager()
        {
            var PlayerCommands = new PlayerCommands();

            Alt.OnClient("SendLogin", SendLogin );
            Alt.OnClient("LogPlayer", LogPlayer);
            Alt.OnClient("Events_PlayerJoin", Events_PlayerJoin);
            Alt.OnClient("UpdateHungerThirst", UpdateHungerThirst);
            Alt.OnClient("MakePlayer", MakePlayer);
            Alt.OnClient("IWantToDie", IWantToDie);
            Alt.OnClient("ImGod", ReviveEvent);
            Alt.OnClient("setGender", (IPlayer client, object[] args) => { client.Model = ((Convert.ToInt32( args[0]) == 1) ? Alt.Hash("mp_f_freemode_01") : Alt.Hash("mp_m_freemode_01")); });
            Alt.OnClient("setCreatorPos", async (IPlayer client, object[] args) => { await client.SetPositionAsync(new Vector3(402.8664f, -996.4108f, -99.00027f)); });
            Alt.OnClient("OpenXtremPlayer", OpenXtremPlayer);

            AltAsync.OnClient("OnKeyPress", OnKeyPress);

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
        private async void MakePlayer(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;
            PlayerHandler ph = new PlayerHandler(client);

            try
            {
                ph.Character = JsonConvert.DeserializeObject<Models.PlayerCustomization>((string)args[0]);
                //ph.Clothing = new Clothings(client);
                ph.Identite = JsonConvert.DeserializeObject<Models.Identite>( (string)args[1], new JsonSerializerSettings { DateParseHandling = DateParseHandling.DateTime } );
            } catch ( Exception ex) {
                Alt.Server.LogWarning("Character Creator Error | " + ex.Data);
                await client.KickAsync("Character Creator Error");
            }

            await client.EmitAsync("FadeOut", 0);
            await Database.MongoDB.Insert("players", ph);
            await ph.LoadPlayer(client, true);
        }

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
                await OpenCreator(client);
            }
        }

        private async Task OnKeyPress(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var ph = client.GetPlayerHandler();

            if (ph != null && ph.OnKeyPressed != null)
                await ph.OnKeyPressed.Invoke(client, (ConsoleKey)(Int64)args[0]);
        }
        #endregion

        #region Methods 
        public static async Task OpenCreator(IPlayer client)
        {
            await client.SetPositionAsync(new Vector3(402.8664f, -996.4108f, -99.00027f));
            client.Rotation = new Vector3(0, 0, -185f);
            client.Emit("OpenCreator");
        }
        public static async Task<PlayerHandler> GetPlayerHandlerDatabase(string socialClub) =>
            await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();

        public static async Task<bool> PlayerHandlerExist(IPlayer player)
        {
            try
            {
                player.GetData("SocialClub", out string social);
                return await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID == social).AnyAsync();
            }
            catch (Exception ex)
            {
                //await player.SendNotificationError("Erreur avec votre compte, contactez un membre du staff.");
                Alt.Server.LogError("PlayerHandlerExist" + ex);
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

        private async void IWantToDie(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

/*            PlayerHandler ph = GetPlayerByClient(client); TODO
            if (ph != null)
            {
                if (GameMode.Instance.FactionManager.Onu != null && GameMode.Instance.FactionManager.Onu.ServicePlayerList.Count > 0)
                {
                    ph.PocketInventory.Clear();
                    await ph.HasMoney(ph.Money);
                }

                await ph.UpdateHungerThirst(100, 100);
                await client.SpawnAsync(new Vector3(308.2974f, -567.4647f, 43.29008f));
                await client.SetRotationAsync(new Rotation(0, 239.0923f, 0));
                await client.Resurrect();
                ph.PlayerSync.Injured = false;
                ph.Health = 100; TODO
            }*/
        }

        public async void ReviveEvent(IPlayer client, object[] args )
        {
            if (!client.Exists)
                return;

            await Revive(client);
        }
        public static async Task Revive(IPlayer client)
        {
            await AltAsync.Do(async () =>
            {
                await client.SpawnAsync(new Position(client.GetPosition().X, client.GetPosition().Y, client.GetPosition().Z));
                await client.SetRotationAsync(client.Rotation);
                await client.SetHealthAsync(5);
            });

            await client.Resurrect();
            var ph = GetPlayerByClient(client);
            //if (ph != null)
                //await ph.SetDead(false); TODO
/*
            if (GameMode.Instance.FactionManager.Onu != null && GameMode.Instance.FactionManager.Onu.ServicePlayerList?.Count > 0)
            {
                foreach (var medecin in await GameMode.Instance.FactionManager.Onu?.GetEmployeeOnline())
                {
                    await medecin.CallAsync("ONU_BlesseEnd", client.Id);
                }
            }*/
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
        private async void OpenXtremPlayer(IPlayer client, object[] args)
        {
            if (uint.TryParse(args[0].ToString(), out uint playerID))
            {
                if (!client.Exists)
                    return;
                var players = Alt.GetAllPlayers();
                foreach(IPlayer player in players)
                {
                    if(player.Id == playerID)
                        if (player != null) await GetPlayerByClient(client)?.OpenXtremPlayer(player);
                }
                
            }
        }

        public static bool HasVehicleKey(IPlayer client, string plate) 
            => client.GetPlayerHandler().ListVehicleKey.Exists(x => x.Plate == plate);

        #endregion
    }
}
