using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using SaltyServer;
using ResurrectionRP_Server.Database;
using ResurrectionRP_Server.Radio;
using ResurrectionRP_Server.Farms;
using AltV.Net.Async;
using AltV.Net.Data;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Services;
using ResurrectionRP_Server.Illegal;
using ResurrectionRP_Server.Entities.Worlds;
using ResurrectionRP_Server.Jobs;
using System.IO;
using ResurrectionRP_Server.DrivingSchool;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server
{
    public class GameMode
    {
        #region Fields
        public ObjectId _id;

        [BsonIgnore]
        public static GameMode Instance { get; private set; }

        [BsonIgnore]
        public static bool IsDebug { get; private set; } = true;

        [BsonIgnore]
        public static bool IsDevServer { get; private set; } = false;

        public static float StreamDistance { get; private set; } = 500;

        [BsonIgnore]
        public static List<IPlayer> PlayerList = new List<IPlayer>();

        public const short GlobalDimension = 0;

        public uint DatabaseVersion { get; set; }

        #region Pools

        public Economy.Economy Economy { get; private set; }

        public static bool ServerLock;

        public Time Time { get; set; }

        [BsonIgnore]
        public bool AutoPound { get; internal set; }
        
        #endregion

        #region Static
        public static Location FirstSpawn = new Location(new Vector3(-1072.886f, -2729.607f, 0.8148939f), new Vector3(0, 0, 313.7496f));
        #endregion

        #endregion

        #region Constructor
        public GameMode()
        {
            if (Instance != null) return;
            Instance = this;
            var ci = new CultureInfo("fr-FR");
            CultureInfo.DefaultThreadCurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => OnServerStop();
        }
        #endregion

        #region Events
        private void OnServerStop()
        {
            AltAsync.Do(() =>
            {
                /*
                foreach (IPlayer player in PlayerList.ToArray())
                {
                    if (player != null && player.Exists)
                        player.Kick("Server stop");
                }*/

                //await HouseManager.House_Exit();
            }).Wait();
        }

        public void OnStart()
        {
            IsDebug = Config.GetSetting<bool>("Debug");
            IsDevServer = Config.GetSetting<bool>("DevServer");
            PlayerManager.StartBankMoney = Config.GetSetting<int>("BankMoneyStart");
            PlayerManager.StartMoney = Config.GetSetting<int>("MoneyStart");

            if (DataMigration.DATABASE_VERSION > Instance.DatabaseVersion)
            {
                Task.Run(async () =>
                {
                    if (!await DataMigration.MigrateDatabase())
                    {
                        Alt.Server.LogError("Error migrating database to newer version");
                        Environment.Exit(1);
                    }
                });
            }

            Alt.OnPlayerConnect += OnPlayerConnected;
            Alt.OnPlayerDisconnect += OnPlayerDisconnected;
            Alt.OnConsoleCommand += Alt_OnConsoleCommand;
 
            Alt.Server.LogColored("~g~Création des controlleurs...");
            Streamer.Streamer.Init();
            ColshapeManager.Init();
            Voice.Init();
            PlayerManager.Init();
            PlayerKeyHandler.Init();
            Events.Initialize();
            VehiclesManager.Init();
            HouseManager.Init();
            Teleport.TeleportManager.Init();
            Inventory.RPGInventoryManager.Init();
            MenuManager.Init();
            XMenuManager.XMenuManager.Init();
            RadioManager.Init();
            Weather.WeatherManager.InitWeather();
            Business.Business.AddCommands();
            Utils.TopServer.Vote.InitVotePlugin();

            Economy = new Economy.Economy();
            Alt.Server.LogColored("~g~Création des controlleurs terminée");

            if (Time == null)
                Time = new Time();

            Alt.Server.LogColored("~g~Initialisations des controlleurs...");

            VehiclesManager.LoadAllVehicles();
            Loader.BusinessesManager.LoadAllBusinesses();
            BanManager.Init();
            HouseManager.LoadAllHouses();
            Factions.FactionManager.InitAllFactions();
            Society.SocietyManager.LoadAllSociety();
            //JobsManager.Init();
            HandlingManager.LoadAllHandling();
            IllegalManager.InitAll();
            Loader.CarParkLoader.LoadAllCarParks();

            Task.Run(async () =>
            {
                try
                {
                    await Discord.Init();

                    Alt.Server.LogColored("~g~Serveur chargé!");
                    Startup.ServerLoaded = true;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError(ex.ToString());
                }
            });
            
            Pound.Init();
            Loader.CarDealerLoaders.LoadAllCardealer();
            DrivingSchoolManager.Load();
            Loader.ClothingLoader.LoadAllCloth();
            Loader.TattooLoader.TattooLoader.LoadAllTattoo();
            Loader.VehicleRentLoaders.LoadAllVehicleRent();
            FarmManager.InitAll();
            Weather.WeatherManager.InitWeather();
            Phone.PhoneManager.Init();
            //TrainManager.LoadTrains();
            

            Alt.Server.LogColored("~g~Initialisation des controlleurs terminé");

            Utils.Util.SetInterval(async () => await Save(), 15000);
            Utils.Util.SetInterval(async () => await Factions.FactionManager.Update(), 60000);

            if (!IsDevServer)
                Utils.Util.SetInterval(async () => await Restart(), 1000);
            
            Utils.Util.SetInterval(() => Time.Update(), 1000);

            bool Hunger = false;
            Utils.Util.SetInterval(async () => {
                Hunger = !Hunger;
                foreach (PlayerHandler ph in PlayerManager.GetPlayersList())
                {
                    if (ph.Client == null)
                        continue;

                    if (!await ph.Client.ExistsAsync())
                        continue;

                    if (Hunger && ph.Hunger > 0)
                        ph.Hunger--;
                    else if (!Hunger && ph.Thirst > 0)
                        ph.Thirst--;

                    if ((ph.Hunger <= 0 || ph.Thirst <= 0) && !ph.IsInComa)
                        await AltAsync.Do(() => ph.SetHealth((ushort)(ph.Client.Health - 25)));
                    else
                        ph.Client.EmitLocked("UpdateHungerThirst", ph.Hunger, ph.Thirst);
                }
            }, 1000 * 60 * 3 / 2 );
                
            Chat.Initialize();

            Chat.RegisterCmd("coords", (IPlayer player, string[] args) =>
            {
                if (player.GetStaffRank() < Utils.Enums.StaffRank.Helper)
                    return;

                if (args.Length == 0)
                {
                    Chat.SendChatMessage(player, "Vous devez renseigné un nom pour la coordonnée");
                    return;
                }

                Vector3 pos = (player.Vehicle != null) ? player.Vehicle.Position : player.Position;
                Vector3 rot = (player.Vehicle != null) ? player.Vehicle.Rotation : player.Rotation;

                string pPosX = pos.X.ToString().Replace(',', '.');
                string pPosY = pos.Y.ToString().Replace(',', '.');
                string pPosZ = pos.Z.ToString().Replace(',', '.');

                string pRotX = rot.X.ToString().Replace(',', '.');
                string pRotY = rot.Y.ToString().Replace(',', '.');
                string pRotZ = rot.Z.ToString().Replace(',', '.');

                string coordsName = args[0];

                StreamWriter coordsFile;
                if (!File.Exists("SavedCoords.txt"))
                {
                    coordsFile = new StreamWriter("SavedCoords.txt");
                }
                else
                {
                    coordsFile = File.AppendText("SavedCoords.txt");
                }

                var data = $"| {coordsName} | Saved Coordenates: new Vector3({pPosX}f,{pPosY}f,{pPosZ}f), new Vector3({pRotX}f,{pRotY}f,{pRotZ}f) | Heading: {player.HeadRotation} | InVehicle: {(player.Vehicle != null)}";
                Chat.SendChatMessage(player, data);
                coordsFile.WriteLine(data);
                coordsFile.Close();
            });

            Chat.RegisterCmd("dimension", (IPlayer player, string[] args) =>
            {
                Alt.Server.LogInfo("My dimension: " + player.Dimension) ;
            });

            Chat.RegisterCmd("save", (IPlayer player, string[] args) =>
            {
                player.GetPlayerHandler()?.UpdateFull();
                player.Vehicle?.GetVehicleHandler()?.UpdateInBackground();
            });

            Chat.RegisterCmd("anim", (IPlayer player, string[] args) =>
            {
                if (player.GetPlayerHandler()?.StaffRank <= 0)
                    return;


                if (args == null || args.Length < 2)
                {
                    player.SendNotificationError("Syntaxe : /anim Dict Anim");
                    return;
                }
                else
                {
                    
                    player.PlayAnimation(args[0], args[1], 8, -1, 5000, (Utils.Enums.AnimationFlags)1);
                }

            });

            Chat.RegisterCmd("tpto", (IPlayer player, string[] args) =>
            {
                if (player.GetPlayerHandler()?.StaffRank <= 0)
                    return;

                float x, y, z = 0;

                if (args == null || args.Length < 3)
                {
                    player.SendNotificationError("Syntaxe : /tpto X Y Z");
                    return;
                }
                else if (!float.TryParse(args[0].ToString(), out x) || !float.TryParse(args[1].ToString(), out y) || !float.TryParse(args[2].ToString(), out z))
                {
                    player.SendNotificationError("Paramètre(s) invalide(s)");
                    return;
                }

                player.Position = new Position(x, y, z);
            });

            Chat.RegisterCmd("gethandling", (IPlayer player, string[] args) =>
            {
                HandlingManager.GetAllHandling(player);
            });

            Chat.RegisterCmd("makehandling", (IPlayer player, string[] args) =>
            {
                if (player.Vehicle == null)
                {
                    player.SendNotificationError("Vous devez être dans un véhicule.");
                    return;
                }

                var vh = player.Vehicle.GetVehicleHandler();
                vh.VehicleData.CustomHandling = vh.VehicleHandling;
            });
        }

        private void Alt_OnConsoleCommand(string name, string[] args)
        {
            if (name == "/say")
            {
                string text = "";

                foreach(string a in args)
                    text += " " + a;
                Alt.EmitAllClients("AnnonceGlobal", text, "AVIS A LA POPULATION!", "ANNONCE SERVEUR");
            }
        }

        private void OnPlayerConnected(IPlayer player, string reason)
        {
            lock (PlayerList)
            {
                if (PlayerList.Find(b => b == player) == null)
                    PlayerList.Add(player);

                Alt.Log($"==> {player.Name} has connected.");
            }
        }

        private void OnPlayerDisconnected(IPlayer player, string reason)
        {
            if (PlayerList.Find(b => b == player) != null)
                PlayerList.Remove(player);

            IllegalManager.OnPlayerDisconnected(player);
            PlayerManager.OnPlayerDisconnected(player, reason);   
        }

        #endregion

        #region Methods
        private bool advert;

        public async Task Save()
        {
            await Database.MongoDB.Update(this, "gamemode", _id);
        }

        public async Task Restart()
        {
            if (DateTime.Now.Hour == 7 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                await Database.MongoDB.Update(this, "gamemode", _id);
                Environment.Exit(0);
            }

            if (DateTime.Now.Hour == 13 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                await Database.MongoDB.Update(this, "gamemode", _id);
                Environment.Exit(0);
            }

            if (DateTime.Now.Hour == 20 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                await Database.MongoDB.Update(this, "gamemode", _id);
                Environment.Exit(0);
            }

            if (DateTime.Now.Hour == 1 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                await Database.MongoDB.Update(this, "gamemode", _id);
                Environment.Exit(0);
            }

            if (advert)
                return;

            if (DateTime.Now.Hour == 6 && DateTime.Now.Minute == 55)
            {
                Alt.EmitAllClients(Utils.Enums.Events.AnnonceGlobal, "COUVRE FEU DANS 5MINUTES!", "AVIS A LA POPULATION!", "COUVRE FEU DANS 5MINUTES!");
                advert = true;
            }

            if (DateTime.Now.Hour == 12 && DateTime.Now.Minute == 55)
            {
                Alt.EmitAllClients(Utils.Enums.Events.AnnonceGlobal, "COUVRE FEU DANS 5MINUTES!", "AVIS A LA POPULATION!", "COUVRE FEU DANS 5MINUTES!");
                advert = true;
            }

            if (DateTime.Now.Hour == 19 && DateTime.Now.Minute == 55)
            {
                Alt.EmitAllClients(Utils.Enums.Events.AnnonceGlobal, "COUVRE FEU DANS 5MINUTES!", "AVIS A LA POPULATION!", "COUVRE FEU DANS 5MINUTES!");
                advert = true;
            }

            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 55)
            {
                Alt.EmitAllClients(Utils.Enums.Events.AnnonceGlobal, "COUVRE FEU DANS 5MINUTES!", "AVIS A LA POPULATION!", "COUVRE FEU DANS 5MINUTES!");
                advert = true;
            }
        }
        #endregion
    }
}
