using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
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

namespace ResurrectionRP_Server
{
    public class GameMode
    {
        #region Fields
        public ObjectId _id;

        [BsonIgnore]
        public static GameMode Instance { get; private set; }

        [BsonIgnore]
        public static bool IsLinux { get; private set; }

        [BsonIgnore]
        public static bool IsDebug { get; private set; } = true;

        [BsonIgnore]
        public bool ServerLoaded = false;

        public static float StreamDistance { get; private set; } = 500;

        [BsonIgnore]
        public static List<IPlayer> PlayerList = new List<IPlayer>();

        public const short GlobalDimension = 0;

        public uint DatabaseVersion { get; set; }

        #region Pools

        [BsonIgnore]
        public Economy.Economy Economy { get; private set; }

        [BsonIgnore]
        public DrivingSchool.DrivingSchoolManager DrivingSchoolManager { get; private set; }

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
                foreach (IPlayer player in PlayerList.ToArray())
                {
                    if (player != null && player.Exists)
                        player.Kick("Server stop");
                }

                //await HouseManager.House_Exit();
            });
        }

        public void OnStart()
        {
            IsDebug = Config.GetSetting<bool>("Debug");
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
 
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
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
            Phone.PhoneManager.Init();

            Economy = new Economy.Economy();
            //DrivingSchoolManager = new DrivingSchool.DrivingSchoolManager();
            Alt.Server.LogColored("~g~Création des controlleurs terminée");

            if (Time == null)
                Time = new Time();

            Alt.Server.LogColored("~g~Initialisations des controlleurs...");
            Task.Run(async () =>
            {
                await BanManager.Init();
                await VehiclesManager.LoadAllVehicles();
                await Loader.CarParkLoader.LoadAllCarPark();  
                await Factions.FactionManager.InitAllFactions();
                await Loader.BusinessesManager.LoadAllBusinesses();         
                await Society.SocietyManager.LoadAllSociety();

                // if (IsDebug)
                //     await IllegalManager.InitAll();

                //await JobsManager.Init();
                await HouseManager.LoadAllHouses();
                
                Alt.Server.LogColored("~g~Serveur chargé!");
                ServerLoaded = true;
            });

            
            Pound.Init();
            Loader.CarDealerLoaders.LoadAllCardealer();
            // DrivingSchoolManager.InitAll();
            Loader.ClothingLoader.LoadAllCloth();
            Loader.TattooLoader.TattooLoader.LoadAllTattoo();
            Loader.VehicleRentLoaders.LoadAllVehicleRent();
            FarmManager.InitAll();
            Weather.WeatherManager.InitWeather();
            Phone.PhoneManager.Init();
            Jobs.JobsManager.Init();

            Alt.Server.LogColored("~g~Initialisation des controlleurs terminé");

            Utils.Utils.SetInterval(async () => await Save(), 15000);
            Utils.Utils.SetInterval(async () => await Factions.FactionManager.Update(), 60000);
            Utils.Utils.SetInterval(async () => await Restart(), 1000);
            
            Utils.Utils.SetInterval(() => Time.Update(), 1000);           

            Chat.Initialize();

            Chat.RegisterCmd("coords", (IPlayer player, string[] args) =>
            {
                if (player.IsInVehicle)
                {
                    Chat.SendChatMessage(player, "X: " + player.Vehicle.Position.X + " Y: " + player.Vehicle.Position.Y + " Z: " + player.Vehicle.Position.Z + " Dim: " + player.Dimension);
                    Chat.SendChatMessage(player, "RX: " + player.Vehicle.Rotation.Roll + " RY: " + player.Vehicle.Rotation.Pitch + " RZ: " + player.Vehicle.Rotation.Yaw);
                }
                else
                {
                    Chat.SendChatMessage(player, "X: " + player.Position.X + " Y: " + player.Position.Y + " Z: " + player.Position.Z + " Dim: " + player.Dimension);
                    Chat.SendChatMessage(player, "RX: " + player.Rotation.Roll + " RY: " + player.Rotation.Pitch + " RZ: " + player.Rotation.Yaw);
                }
            });

            Chat.RegisterCmd("getCoords", (IPlayer player, string[] args) =>
            {
                Alt.Server.LogColored($" X: {player.Position.X}  Y: {player.Position.Y} Z: {player.Position.Z} ");
                Alt.Server.LogColored($" RX: {player.Rotation.Roll}  RY: {player.Rotation.Pitch} RZ: {player.Rotation.Yaw} ");
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

            Chat.RegisterCmd("tpto", (IPlayer player, string[] args) =>
            {
                if (player.GetPlayerHandler()?.StaffRank <= 0)
                    return;

                player.Position = (new Position(Convert.ToSingle(args[0]), Convert.ToSingle(args[1]), Convert.ToSingle(args[2])));
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
