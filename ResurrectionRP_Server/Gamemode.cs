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
        public static bool IsLinux { get; private set; }

        [BsonIgnore]
        public bool IsDebug { get; private set; } = true;

        [BsonIgnore]
        public bool ServerLoaded = false;

        [BsonIgnore]
        public Streamer.Streamer Streamer { get; private set; }

        [BsonIgnore]
        public float StreamDistance { get; private set; } = 500;

        [BsonIgnore]
        public BanManager BanManager { get; private set; }

        [BsonIgnore]
        public PlayerManager PlayerManager { get; private set; }

        [BsonIgnore]
        public List<IPlayer> PlayerList = new List<IPlayer>();

        public const short GlobalDimension = short.MaxValue;

        public uint DatabaseVersion { get; set; }

        #region Pools

        [BsonIgnore]
        public Economy.Economy Economy { get; private set; }

        [BsonIgnore]
        public VehiclesManager VehicleManager { get; private set; }
        public Services.Pound PoundManager { get; set; }

        [BsonIgnore]
        public Jobs.JobsManager JobsManager { get; private set; }

        [BsonIgnore]
        public Factions.FactionManager FactionManager { get; private set; }

        // Menus
        [BsonIgnore]
        public MenuManager MenuManager { get; private set; }
        [BsonIgnore]
        public XMenuManager.XMenuManager XMenuManager { get; private set; }

        [BsonIgnore]
        public Entities.Peds.PedsManager PedManager { get; private set; }

        [BsonIgnore]
        public Entities.Blips.BlipsManager BlipsManager { get; private set; }

        [BsonIgnore]
        public Entities.Objects.ObjectManager ObjectManager { get; private set; }

        [BsonIgnore]
        public ResuPickupManager ResuPickupManager { get; private set; }

        [BsonIgnore]
        public Loader.BusinessesLoader BusinessesManager { get; private set; }

        [BsonIgnore]
        public Society.SocietyManager SocietyManager { get; private set; }

        [BsonIgnore]
        public DrivingSchool.DrivingSchoolManager DrivingSchoolManager { get; private set; }

        [BsonIgnore]
        public Teleport.TeleportManager TeleportManager { get; private set; }

        [BsonIgnore]
        public Utils.DoorManager DoorManager { get; private set; }

        [BsonIgnore]
        public Weather.WeatherManager WeatherManager { get; private set; }
        [BsonIgnore]
        public Inventory.RPGInventoryManager RPGInventory { get; private set; }
        [BsonIgnore]
        public Phone.PhoneManager PhoneManager { get; private set; }

        [BsonIgnore]
        public Voice VoiceController { get; private set; }
        [BsonIgnore]
        public RadioManager RadioManager { get; private set; }
        [BsonIgnore]
        public LifeInvader LifeInvader { get; private set; }
        [BsonIgnore]
        public HouseManager HouseManager { get; private set; }

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
        private Task OnServerStop()
        {
            var players = GameMode.Instance.PlayerList;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].Exists)
                    players[i].Kick("Server stop");
            }

            //await HouseManager.House_Exit();
            return Task.CompletedTask;
        }

        public async Task OnStartAsync()
        {
            IsDebug = Config.GetSetting<bool>("Debug");
            PlayerManager.StartBankMoney = Config.GetSetting<int>("BankMoneyStart");
            PlayerManager.StartMoney = Config.GetSetting<int>("MoneyStart");

            if (DataMigration.DATABASE_VERSION > Instance.DatabaseVersion)
            {
                if (!await DataMigration.MigrateDatabase())
                {
                    Alt.Server.LogError("Error migrating database to newer version");
                    Environment.Exit(1);
                }
            }

            AltAsync.OnPlayerConnect += OnPlayerConnected;
            AltAsync.OnPlayerDisconnect += OnPlayerDisconnected;

            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            Alt.Server.LogColored("~g~Création des controlleurs...");
            Streamer = new Streamer.Streamer();
            Economy = new Economy.Economy();
            DoorManager = new Utils.DoorManager();
            PlayerManager = new PlayerManager();
            BanManager = new BanManager();
            VehicleManager = new VehiclesManager();
            PedManager = new Entities.Peds.PedsManager();
            BlipsManager = new Entities.Blips.BlipsManager();
            ObjectManager = new Entities.Objects.ObjectManager();
            PhoneManager = new Phone.PhoneManager();
            FactionManager = new Factions.FactionManager();
            RPGInventory = new Inventory.RPGInventoryManager();
            SocietyManager = new Society.SocietyManager();
            MenuManager = new MenuManager();
            ResuPickupManager = new ResuPickupManager();
            TeleportManager = new Teleport.TeleportManager();
            BusinessesManager = new Loader.BusinessesLoader();
            XMenuManager = new XMenuManager.XMenuManager();
            WeatherManager = new Weather.WeatherManager();
            DrivingSchoolManager = new DrivingSchool.DrivingSchoolManager();
            JobsManager = new Jobs.JobsManager();
            VoiceController = new Voice();
            HouseManager = new HouseManager();

            RadioManager = new RadioManager();
            LifeInvader = new LifeInvader();
            //FactionManager = new FactionManager();
            Alt.Server.LogColored("~g~Création des controlleurs terminée");

            if (Time == null)
                Time = new Time();

            if (PoundManager == null)
                PoundManager = new Services.Pound();

            Alt.Server.LogColored("~g~Initialisations des controlleurs...");
            await VehiclesManager.LoadAllVehicles();
            await Loader.CarParkLoader.LoadAllCarPark();
            await Loader.CarDealerLoaders.LoadAllCardealer();
            await Loader.VehicleRentLoaders.LoadAllVehicleRent();
            await Loader.TattooLoader.TattooLoader.LoadAllTattoo();
            await FactionManager.InitAllFactions();
            await Loader.ClothingLoader.LoadAllCloth();
            await Loader.BusinessesLoader.LoadAllBusinesses();
            await WeatherManager.InitWeather();
            await Society.SocietyManager.LoadAllSociety();
            await JobsManager.Init();
            await PoundManager.LoadPound();
            await FarmManager.InitAll();        
            await HouseManager.LoadAllHouses();

            DrivingSchoolManager.InitAll();
            LifeInvader.Load();
            VoiceController.OnResourceStart();
            Alt.Server.LogColored("~g~Initialisation des controlleurs terminé");

            AutoPound = Config.GetSetting<bool>("AutoPound");

            if (AutoPound)
                PoundManager.Price = 0;

            Events.Initialize();

            Utils.Utils.Delay(15000, false, async () => await Save());
            Utils.Utils.Delay(1000, false, () => Time.Update());
            Utils.Utils.Delay(60000, false, async () => await FactionManager.Update());
            Utils.Utils.Delay(1000, false, () => VehiclesManager.UpdateVehiclesMilageAndFuel());

            Chat.Initialize();

            Chat.RegisterCmd("coords", (IPlayer player, string[] args) =>
            {
                if (player.Vehicle != null)
                {
                    Chat.SendChatMessage(player, "X: " + player.Vehicle.Position.X + " Y: " + player.Vehicle.Position.Y + " Z: " + player.Vehicle.Position.Z);
                    Chat.SendChatMessage(player, "RX: " + player.Vehicle.Rotation.Roll + " RY: " + player.Vehicle.Rotation.Pitch + " RZ: " + player.Vehicle.Rotation.Yaw);
                }
                else
                {
                    Chat.SendChatMessage(player, "X: " + player.Position.X + " Y: " + player.Position.Y + " Z: " + player.Position.Z);
                    Chat.SendChatMessage(player, "RX: " + player.Rotation.Roll + " RY: " + player.Rotation.Pitch + " RZ: " + player.Rotation.Yaw);
                }
                return Task.CompletedTask;
            });

            Chat.RegisterCmd("getCoords", (IPlayer player, string[] args) =>
            {
                Alt.Server.LogColored($" X: {player.Position.X}  Y: {player.Position.Y} Z: {player.Position.Z} ");
                Alt.Server.LogColored($" RX: {player.Rotation.Roll}  RY: {player.Rotation.Pitch} RZ: {player.Rotation.Yaw} ");
                return Task.CompletedTask;
            });

            Chat.RegisterCmd("dimension", (IPlayer player, string[] args) =>
            {
                Alt.Server.LogInfo("My dimension: " + player.Dimension) ;
                return Task.CompletedTask;
            });

            Chat.RegisterCmd("save", (IPlayer player, string[] args) =>
            {
                player.GetPlayerHandler()?.Update();
                player.Vehicle?.GetVehicleHandler()?.UpdateFull();
                return Task.CompletedTask;
            });
            Chat.RegisterCmd("tpto", async (IPlayer player, string[] args) =>
            {
                if (player.GetPlayerHandler()?.StaffRank <= 0)
                    return;
                await player.SetPositionAsync(new Position(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2])));
            });


            ServerLoaded = true;
        }

        private Task OnPlayerConnected(IPlayer player, string reason)
        {
            if (PlayerList.Find(b => b == player) == null)
                PlayerList.Add(player);

            Alt.Log($"==> {player.Name} has connected.");
            return Task.CompletedTask;
        }

        private async Task OnPlayerDisconnected(ReadOnlyPlayer player, IPlayer origin, string reason)
        {
            if (PlayerList.Find(b => b == origin) != null)
                PlayerList.Remove(origin);

            await PlayerManager.OnPlayerDisconnected(player, origin, reason);   
        }
        #endregion

        #region Methods
        public async Task Save()
        {
            await Database.MongoDB.Update(this, "gamemode", _id);
        }
        #endregion
    }
}
