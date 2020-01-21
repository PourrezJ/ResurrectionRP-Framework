using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Driver;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities.Worlds;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Illegal;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using WordPressPCL;
using WordPressPCL.Models;

namespace ResurrectionRP_Server.Entities.Players
{
    public static class PlayerManager
    {
        #region Variables 
        private readonly static Location charpos = new Location(new Vector3(402.8664f, -996.4108f, -99.00027f), new Vector3(0,0,60));

        private static ConcurrentDictionary<string, PlayerHandler> allplayerHandlers = new ConcurrentDictionary<string, PlayerHandler>();


        public static List<DeadPlayer> DeadPlayers = new List<DeadPlayer>();

        private static short Dimension = short.MinValue;
        public static int StartMoney = 0;
        public static int StartBankMoney = 0;
        #endregion

        #region Constructor
        public static void Init()
        {
            new PlayerCommands();
            new VehicleCommands();
            new FactionsCommands();
            new HouseCommands();
            new IllegalCommands();
            new Society.Commands();

                 
            
            AltAsync.OnClient <IPlayer, string>("SendLogin", SendLogin);

            Alt.OnClient<IPlayer, string, string>("MakePlayer", MakePlayer);
            Alt.OnClient<IPlayer, string, string>("Events_PlayerJoin", OnPlayerJoin);
            Alt.OnClient<IPlayer>("LogPlayer", LogPlayer);
            Alt.OnClient<IPlayer>("IWantToDie", IWantToDie);
            Alt.OnClient("Player_SetInComa", (IPlayer client) => client.GetPlayerHandler().IsInComa = true); // peut être mieux?
            Alt.OnClient("ExitGame", (IPlayer client) => client.Kick("Exit"));

            Alt.Server.LogInfo("--- Start loading all players from database ---");
            var players = Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").AsQueryable();

            foreach(var player in players)
            {
                // Todo: ajouter un check nombre de temps sans avoir jouer?
                allplayerHandlers.TryAdd(player.PID, player);
            }
            Alt.Server.LogInfo($"--- Finish loading all players from database: {players.Count()} ---");

            Utils.Utils.SetInterval(() =>
            {
                var pls = PlayerHandler.PlayerHandlerList.ToList();

                for (int i = 0; i < pls.Count; i++)
                {
                    var ph = pls[i];

                    if (!ph.Key.Exists)
                        continue;

                    if (GameMode.PlayerList.Any(v => v.Id == ph.Key.Id))
                    {
                        if (ph.Value != null)
                            ph.Value.UpdateFull();
                    }
                }

                pls.Clear();
            }, 300000);
        }

        #endregion

        #region ServerEvents
        public static void OnPlayerDisconnected(IPlayer player, string reason)
        {
            PlayerHandler.PlayerHandlerList.TryGetValue(player, out PlayerHandler ph);

            if (ph == null)
            {
                Alt.Server.LogInfo($"Joueur social: {player.Name} {player.SocialClubId} est déconnecté raison: {reason}.");
            }
            else
            {
                ph.IsOnline = false;
                MenuManager.OnPlayerDisconnect(player);
                FactionManager.OnPlayerDisconnected(player);
                TrainManager.OnPlayerDisconnected(player);

                if (Phone.PhoneManager.PhoneClientList.ContainsKey(player))
                    Phone.PhoneManager.PhoneClientList.TryRemove(player, out List<Phone.Phone> phoneList);

                if (RPGInventoryManager.HasInventoryOpen(player))
                {
                    var rpg = RPGInventoryManager.GetRPGInventory(player);

                    if (rpg != null)
                        rpg.OnClose?.Invoke(player, rpg);

                    RPGInventoryManager.OnPlayerQuit(player);
                }

                if (HouseManager.IsInHouse(player))
                {
                    House house = HouseManager.GetHouse(player);
                    ph.Location = new Location(house.Position, new Vector3());
                }
                else
                    ph.Location = new Location(player.Position, player.Rotation);

                if ((DateTime.Now - ph.LastUpdate).Minutes >= 1)
                {
                    ph.TimeSpent += (DateTime.Now - ph.LastUpdate).Minutes;
                    ph.LastUpdate = DateTime.Now;
                }

                var dead = DeadPlayers.FindLast(p => p.Victime == player);
                if (dead != null)
                    dead.Remove();

                ph.UpdateInBackground();
                PlayerHandler.PlayerHandlerList.Remove(player, out _);

                if (ph.Vehicle != null)
                {
                    VehicleHandler vh = ph.Vehicle;

                    if (vh.VehicleData.LastDriver == ph.Identite.Name)
                    {
                        vh.LockState = VehicleLockState.Locked;
                        vh.UpdateInBackground();
                    }
                }

                Alt.Server.LogInfo($"Joueur social: {ph.PID} || Nom: {ph.Identite.Name} est déconnecté raison: {reason}.");
            }    
        }

        public static void OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
        {
            if (player.Exists)
            {
                if (weapon != 2725352035)
                {
                    player.Emit("ONU_PlayerDeath", weapon);
                    DeadPlayers.Add(new DeadPlayer(player, killer, weapon));
                }
                else
                {
                    player.SendNotification($"Ne va pas vers la lumière, tu vas te relever.");

                    Utils.Utils.SetInterval(() =>
                    {
                        if (player.Exists)
                            player.Revive(105);
                    }, 60000);
                }

                player.GetPlayerHandler()?.UpdateFull();
            }
        }

        public static void OnPlayerJoin(IPlayer player, string socialclub, string discordData)
        {
            try
            {
                if (!player.Exists)
                    return;

                if (GameMode.ServerLock)
                {
                    lock (player)
                    {
                        player.Emit("FadeIn", 0);
                        player.Kick("Serveur Lock!");
                    }
                }

                if (string.IsNullOrEmpty(socialclub))
                {
                    player.SendNotificationError("Vous avez un problème avec votre socialclub", 60000);
                    return;
                }

                if (!string.IsNullOrEmpty(discordData) && discordData != "null")
                {
                    DiscordData discord = JsonConvert.DeserializeObject<DiscordData>(discordData);

                    if (discord != null)
                    {
                        var userGuildDiscord = Discord.GetSocketGuildUser(ulong.Parse(discord.id));

                        if (userGuildDiscord != null)
                        {
                            if (!Discord.IsCitoyen(userGuildDiscord))
                            {
                                player.SendNotificationError("Vous n'êtes pas whitelist Citoyen sur le discord.", 60000);
                                return;
                            }

                            discord.SocketGuildUser = userGuildDiscord;
                            if (!Discord.DiscordPlayers.ContainsKey(player))
                                Discord.DiscordPlayers.TryAdd(player, discord);
                        }
                    }
                }

                string playerIp = player.Ip;

                Alt.Server.LogInfo($" {socialclub} : ({playerIp}) en attente de connexion.");

                if (socialclub == "UNKNOWN")
                {
                    Alt.Server.LogInfo($"({playerIp}) kick pour problème de social club.");
                    player.Kick("Vous avez un problème avec votre social club.");
                    return;
                }

                if (IsBan(socialclub))
                {
                    Alt.Server.LogInfo($"({playerIp}) est banni.");
                    player.Kick("Vous êtes banni!");
                    return;
                }

                player.Model = (uint)PedModel.FreemodeMale01;
                player.Spawn(new Position(-1072.886f, -2729.607f, 0.8148939f));
                player.Dimension = Dimension++;

                player.SetData("SocialClub", socialclub);


                if (!GameMode.IsDebug)
                {
                    try
                    {
                        if (!Config.GetSetting<bool>("WhitelistOpen"))
                        {
                            player.EmitLocked("OpenLogin");
                            return;
                        }

                        Task.Run(async () =>
                        {
                            Whitelist whitelist = await Whitelist.GetWhitelistFromAPI(socialclub);
                            await AltAsync.Do(() =>
                            {
                                if (whitelist != null && whitelist.Whitelisted)
                                {
                                    if (whitelist.IsBan)
                                    {
                                        if (DateTime.Now > whitelist.EndBanTime)
                                        {
                                            whitelist.IsBan = false;
                                            player.EmitLocked("OpenLogin", socialclub);
                                            return;
                                        }

                                        string _kickMessage = $"Vous êtes ban du serveur jusqu'au {whitelist.EndBanTime.ToShortDateString()}";
                                        player.Kick(_kickMessage);
                                    }
                                    else
                                        player.EmitLocked("OpenLogin", socialclub);
                                }
                                else
                                {
                                    Alt.Server.LogInfo($"({player.Ip}) ({socialclub}) n'est pas whitelist sur le serveur.");
                                    player.EmitLocked("FadeIn", 0);
                                    string _kickMessage = "Vous n'êtes pas whitelist sur le serveur";
                                    player.Kick(_kickMessage);
                                }
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        string _kickMessage = "Vous n'êtes pas whitelist sur le serveur";
                        player.Kick(_kickMessage);
                        Alt.Server.LogError("Player Login" + ex.Data);
                    }
                }
                else
                    ConnectPlayer(player);
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }           
        }
        #endregion

        #region RemoteEvents
        private static void MakePlayer(IPlayer client, string charData, string identite)
        {
            if (!client.Exists)
                return;

            PlayerHandler ph = null;
            try
            {
                ph = new PlayerHandler(client);
                ph.Character = JsonConvert.DeserializeObject<Models.PlayerCustomization>(charData);
                ph.Clothing = new Clothings(client);
                ph.Identite = JsonConvert.DeserializeObject<Models.Identite>(identite, new JsonSerializerSettings { DateParseHandling = DateParseHandling.DateTime } );
            } catch ( Exception ex) {
                Alt.Server.LogWarning("Character Creator Error | " + ex.ToString());
                client.Kick("Character Creator Error");
            }

            if (ph == null)
                return;

            if(ph.PID == null)
            {
                client.SendNotificationError("Vous avez un problème avec votre social-club.", 60000);
                return;
            }

            client.Emit("FadeOut", 0);
            Task.Run(async () => await Database.MongoDB.Insert("players", ph));
            ph.LoadPlayer(client, true);
        }

        private static async void SendLogin(IPlayer client, string datastr)
        {
            if (!await client.ExistsAsync())
                return;

            var definition = new { login = "", password = "", socialClub = "" };

            var data = JsonConvert.DeserializeAnonymousType(datastr, definition);
            var wpclient = new WordPressClient("https://resurrectionrp.fr/wp-json/");
            wpclient.AuthMethod = AuthMethod.JWT;

            try
            {
                await wpclient.RequestJWToken(data.login, data.password);
                if (await wpclient.IsValidJWToken())
                {
                    await client.EmitAsync("LoginOK", await client.PlayerHandlerExist());
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

        private static void LogPlayer(IPlayer client)
        {
            if (!client.Exists)
                return;

            ConnectPlayer(client);
        }

        public static void ConnectPlayer(IPlayer client)
        {
            ulong socialClubId = 0;

            lock (client)
                socialClubId = client.SocialClubId;
            
            if (socialClubId == 0)
            {
                client.Kick("Vous n'êtes pas connecté correctement, redémarrez.");
                Alt.Server.LogWarning(client.Name + " est kick, problème avec sont social club");
                return;
            }

            Task.Run(async () =>
            {
                if (await client.PlayerHandlerExist())
                {
                    await client.EmitAsync("FadeOut", 0);
                    client.GetData("SocialClub", out string social);
                    PlayerHandler player = await GetPlayerHandlerDatabase(social);
                    if (player == null)
                    {
                        client.SendNotificationError("Erreur avec votre personnage.");
                        return;
                    }
                    player.LastUpdate = DateTime.Now;
                    await AltAsync.Do(()=> player.LoadPlayer(client));
                }
                else
                    await client.EmitAsync("OpenCreator");
            });
        }

        #endregion

        #region Methods 
        public static async Task<PlayerHandler> GetPlayerHandlerDatabase(string socialClub) =>
            await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();

        public static PlayerHandler GetPlayerHandlerCache(string socialClub) => allplayerHandlers[socialClub];


        private static void IWantToDie(IPlayer client)
        {
            client.Revive(200, new Vector3(308.2974f, -567.4647f, 43.29008f));

            var ph = client.GetPlayerHandler();

            ph.UpdateHungerThirst(50, 50);
            ph.UpdateFull();
        }

        public static PlayerHandler GetPlayerBySCN(string socialClubName)
        {
            try
            {
                var players = GameMode.PlayerList;
                for (int a = 0; a < players.Count; a++)
                {
                    if (players[a] == null)
                        continue;

                    if (!players[a].Exists)
                        continue;

                    if ((players[a].GetSocialClub()).ToLower() == socialClubName.ToLower())
                        return players[a].GetPlayerHandler();
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("GetPlayerBySCN: " + socialClubName + ex);
            }

            return null;
        }

        public static PlayerHandler GetPlayerByName(string name)
            => GameMode.PlayerList.FirstOrDefault(x => x.Exists && x.GetPlayerHandler()?.Identite?.Name.ToLower() == name.ToLower())?.GetPlayerHandler() ?? null;

        public static bool IsBan(string social)
        {
            foreach(Ban ban in BanManager.BanList)
            {
                if (ban.SocialClub == social)
                    return true;
            }

            return false;
        }

        public static List<PlayerHandler> GetPlayersList()
        {
            List<PlayerHandler> phList = new List<PlayerHandler>();

            foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.Exists && x.GetPlayerHandler() != null))
            {
                if (!player.Exists)
                    continue;
                // TODO Need to add a veritable check
                phList.Add(player.GetPlayerHandler());
            }

            return phList;
        }
        #endregion
    }
}
