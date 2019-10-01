using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Driver;
using Newtonsoft.Json;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using WordPressPCL;
using WordPressPCL.Models;

namespace ResurrectionRP_Server.Entities.Players
{
    public class PlayerManager
    {
        #region Variables 
        private readonly static Location charpos = new Location(new Vector3(402.8664f, -996.4108f, -99.00027f), new Vector3(0,0,60));

        public static List<DeadPlayer> DeadPlayers = new List<DeadPlayer>();

        private static short Dimension = short.MinValue;
        public static int StartMoney = 0;
        public static int StartBankMoney = 0;

        private bool _whitelistOn = false;
        #endregion

        #region Constructor
        public PlayerManager()
        {
            new PlayerCommands();
            new VehicleCommands();
            new FactionsCommands();
            new HouseCommands();

            AltAsync.OnClient("LogPlayer", LogPlayer);
            AltAsync.OnClient("MakePlayer", MakePlayer);
            AltAsync.OnClient("SendLogin", SendLogin );         
            AltAsync.OnClient("Events_PlayerJoin", Events_PlayerJoin);   

            Alt.OnClient("OnKeyPress", OnKeyPress);
            Alt.OnClient("OnKeyUp", OnKeyReleased);
            Alt.OnClient("ExitGame", (IPlayer client, object[] args) => client.Kick("Exit"));

            //Alt.OnClient("UpdateHungerThirst", UpdateHungerThirst);

            AltAsync.OnClient("IWantToDie", IWantToDie);

            
            /*
            Utils.Utils.Delay(300000, false, async () =>
            {
                var players = PlayerHandler.PlayerHandlerList.ToList();

                for (int i = 0; i < players.Count; i++)
                {
                    try
                    {
                        var ph = players[i];

                        if (!ph.Key.Exists)
                            continue;

                        if (GameMode.Instance.PlayerList.Any(v => v.Id == ph.Key.Id))
                        {
                            if (ph.Value != null)
                                await ph.Value.Update();
                        }

                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError(ex.ToString());
                    }
                }

                players.Clear();
            });*/

        }


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
        public async Task OnPlayerDisconnected(ReadOnlyPlayer player, IPlayer origin, string reason)
        {
            PlayerHandler.PlayerHandlerList.TryGetValue(origin, out PlayerHandler ph);

            if (ph == null)
                return;

            ph.IsOnline = false;
            MenuManager.OnPlayerDisconnect(origin);

            if (GameMode.Instance.PhoneManager.PhoneClientList.ContainsKey(origin))
                GameMode.Instance.PhoneManager.PhoneClientList.TryRemove(origin, out List<Phone.Phone> phoneList);

            if (RPGInventoryManager.HasInventoryOpen(origin))
            {
                var rpg = RPGInventoryManager.GetRPGInventory(origin);

                if (rpg != null && rpg.OnClose != null)
                    await rpg.OnClose.Invoke(origin, rpg);

                GameMode.Instance.RPGInventory.OnPlayerQuit(origin);
            }

            if (HouseManager.IsInHouse(origin))
            {
                House house = HouseManager.GetHouse(origin);
                ph.Location = new Location(house.Position, new Vector3());
                house.PlayersInside.Remove(origin);
            }
            else
                ph.Location = new Location(player.Position, player.Rotation);

            if ((DateTime.Now - ph.LastUpdate).Minutes >= 1)
            {
                ph.TimeSpent += (DateTime.Now - ph.LastUpdate).Minutes;
                ph.LastUpdate = DateTime.Now;
            }

            var dead = DeadPlayers.FindLast(p => p.Victime == origin);
            if (dead != null)
                dead.Remove();

            ph.UpdateInBackground();
            PlayerHandler.PlayerHandlerList.Remove(origin, out _);

            if (ph.Vehicle != null)
            {
                VehicleHandler vh = ph.Vehicle;

                if (vh.LastDriver == ph.Identite.Name)
                {
                    vh.LockState = VehicleLockState.Locked;
                    vh.UpdateInBackground();
                }
            }

            Alt.Server.LogInfo($"Joueur social: {ph.PID} || Nom: {ph.Identite.Name} est déconnecté raison: {reason}.");
        }

        public void Alt_OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
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

        public async Task Events_PlayerJoin(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            string socialclub = args[0].ToString();
            DiscordData discord = JsonConvert.DeserializeObject<DiscordData>(args[1].ToString());
            Alt.Server.LogInfo($" {socialclub} : ({player.Ip}) en attente de connexion.");

            if (socialclub == "UNKNOWN")
            {
                Alt.Server.LogInfo($"({player.Ip}) kick pour problème de social club.");
                await player.KickAsync("Vous avez un problème avec votre social club.");
                return;
            }


            if (IsBan(socialclub))
            {
                Alt.Server.LogInfo($"({player.Ip}) est banni.");
                await player.KickAsync("Vous êtes banni!");
                return;
            }

            player.SetData("SocialClub", socialclub);
            await player.SetModelAsync((uint)AltV.Net.Enums.PedModel.FreemodeMale01);
            await player.SpawnAsync(new Position(-1072.886f, -2729.607f, 0.8148939f));

            if (GameMode.ServerLock)
            {
                await player.EmitAsync("FadeIn", 0);
                await player.KickAsync("Serveur Lock!");
            }

            while (!GameMode.Instance.ServerLoaded)
                await Task.Delay(100);

            await player.SetDimensionAsync(Dimension++);

            if (!GameMode.Instance.IsDebug)
            {
                try
                {
                    if (!Config.GetSetting<bool>("WhitelistOpen"))
                    {
                        player.EmitLocked("OpenLogin");
                        return;
                    }

                   Whitelist whitelist = await Whitelist.GetWhitelistFromAPI(socialclub);

                    if (whitelist != null && whitelist.Whitelisted)
                    {
                        if (whitelist.IsBan)
                        {
                            if (DateTime.Now > whitelist.EndBanTime)
                            {
                                whitelist.IsBan = false;
                                player.EmitLocked("OpenLogin", args[0]);
                                return;
                            }

                            string _kickMessage = $"Vous êtes ban du serveur jusqu'au {whitelist.EndBanTime.ToShortDateString()}";
                            await player.KickAsync(_kickMessage);
                        }
                        else
                            player.EmitLocked("OpenLogin", socialclub);
                    }
                    else
                    {
                        Alt.Server.LogInfo($"({player.Ip}) ({socialclub}) n'est pas whitelist sur le serveur.");
                        player.EmitLocked("FadeIn", 0);
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
                await ConnectPlayer(player);
        }
        #endregion

        #region RemoteEvents
        private async Task MakePlayer(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;
            PlayerHandler ph = new PlayerHandler(client);

            try
            {
                ph.Character = JsonConvert.DeserializeObject<Models.PlayerCustomization>((string)args[0]);
                ph.Clothing = new Clothings(client);
                ph.Identite = JsonConvert.DeserializeObject<Models.Identite>( (string)args[1], new JsonSerializerSettings { DateParseHandling = DateParseHandling.DateTime } );
            } catch ( Exception ex) {
                Alt.Server.LogWarning("Character Creator Error | " + ex.Data);
                await client.KickAsync("Character Creator Error");
            }

            await client.EmitAsync("FadeOut", 0);
            await Database.MongoDB.Insert("players", ph);
            await ph.LoadPlayer(client, true);
        }

        private async Task SendLogin(IPlayer client, object[] args)
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

        private void UpdateHungerThirst(IPlayer client, object[] arg)
        {
            if (!client.Exists)
                return;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                ph.Hunger = Convert.ToInt32(arg[0]);
                ph.Thirst = Convert.ToInt32(arg[1]);
                ph.UpdateFull();
            }
        }

        private async Task LogPlayer(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            await ConnectPlayer(client);
        }

        public static async Task ConnectPlayer(IPlayer client)
        {
            if (client.SocialClubId == 0)
                await client.KickAsync("Vous n'êtes pas connecté correctement, redémarrez.");
            if (await client.PlayerHandlerExist())
            {
                await client.EmitAsync("FadeOut",0);
                client.GetData("SocialClub", out string social);
                PlayerHandler player = await GetPlayerHandlerDatabase(social);
                player.LastUpdate = DateTime.Now;
                await player.LoadPlayer(client);
            }
            else
                await client.EmitAsync("OpenCreator");
        }

        private void OnKeyPress(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.OnKeyPressedAsync != null)
                Task.Run(async ()=> await ph.OnKeyPressedAsync.Invoke(client, (ConsoleKey)(Int64)args[0], JsonConvert.DeserializeObject<RaycastData>(args[1].ToString()), (IVehicle)args[2] ?? null, (IPlayer)args[3] ?? null, Convert.ToInt32(args[4])));
            if (ph.OnKeyPressed != null)
                ph.OnKeyPressed(client, (ConsoleKey)(Int64)args[0], JsonConvert.DeserializeObject<RaycastData>(args[1].ToString()), (IVehicle)args[2] ?? null, (IPlayer)args[3] ?? null, Convert.ToInt32(args[4]));

        }

        private void OnKeyReleased(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.OnKeyReleasedAsync != null)
                Task.Run(async () => await ph.OnKeyReleasedAsync.Invoke(client, (ConsoleKey)(Int64)args[0]));
            else if (ph.OnKeyReleased != null)
                ph.OnKeyReleased(client, (ConsoleKey)(Int64)args[0]);

        }
        #endregion

        #region Methods 
        public static async Task<PlayerHandler> GetPlayerHandlerDatabase(string socialClub) =>
            await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();

        private async Task IWantToDie(IPlayer client, object[] args)
        {
            await client.ReviveAsync(200, new Vector3(308.2974f, -567.4647f, 43.29008f));
            client.GetPlayerHandler()?.UpdateFull();
        }

        public static PlayerHandler GetPlayerBySCN(string socialClubName)
        {
            try
            {
                var players = GameMode.Instance.PlayerList;
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
            => GameMode.Instance.PlayerList.FirstOrDefault(x => x.Exists && x.GetPlayerHandler()?.Identite?.Name.ToLower() == name.ToLower())?.GetPlayerHandler() ?? null;

        public static bool IsBan(string social)
        {
            if (GameMode.Instance.BanManager == null)
                return false;

            foreach(Ban ban in GameMode.Instance.BanManager.BanList)
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
